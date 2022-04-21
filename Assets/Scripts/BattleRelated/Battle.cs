using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#region queue_datatypes
public class CombatantInfo : IComparable<CombatantInfo>{
    #region information

    private Character caster;
    private int delay_set;
    private int delay_left;
    private int turn;
    #endregion

    #region accessors
    public Character Caster{
        get{ return caster; }
    }
    public int DelayCurrent{
        get{ return delay_left; }
    }
    public int DelayOriginal{
        get{ return delay_set; }
    }
    public int Turn{
        get{ return turn; }
    }

    public bool Inactive{
        get{ return caster.IsDead; }
    }
    #endregion

    public CombatantInfo(Character c, Move move){
        caster = c;
        SetNewMove(move);
    }

    private CombatantInfo(Character c, int dNow, int dOrg){
        caster = c;
        delay_left = dNow;
        delay_set = dOrg;
    }

    public void SetNewMove(Move move){
        delay_set = caster.GetModifiedDelay(move.Delay);
        delay_left = delay_set;
        turn++;
    }

    public CombatantInfo Copy(){
        return new CombatantInfo(caster, delay_left, delay_set);
    }

    public void ProgressTime(int delay){
        if(caster.IsAlive){
            delay_left -= delay;
        }
    }

    public int CompareTo(CombatantInfo other){
        // first make sure neither of them are dead
        if(Inactive || other.Inactive){
            if(Inactive && other.Inactive){
                return 0;
            }
            return (Inactive)?-1:1;
        }
        // afterwards, first check delay left
        int result = other.DelayCurrent - delay_left ;
        if(result != 0){ return result; }
        // then check turn
        result = other.Turn - turn;
        if(result != 0){ return result; }
        // finally check for who had the longer wait
        return other.DelayOriginal - delay_set;
    }
}
#endregion

public class Battle : MonoBehaviour
{
    [SerializeField]
    private UIQueue ui_queue;

    [SerializeField]
    private Character[] combatants;

    private List<Character> join_queue;

    private bool ready;
    private List<CombatantInfo> combatant_turn_info;
    private int turnState;
    [SerializeField]
    private Move default_move;

    public Move DefaultMove{
        get{ return default_move; }
    }

    public bool IsReady{
        get { return ready; }
    }

    public int NumCombatants {
        get { return combatant_turn_info.Count; }
    }

    public bool HasConflict{
        get{
            List<Faction> factionsInConflict = new List<Faction>();
            foreach(CombatantInfo ci in combatant_turn_info){
                if(ci.Caster.IsDead){
                    continue;
                }
                bool dne = true;
                Faction faction = ci.Caster.Faction;
                for(int i = 0; dne && i < factionsInConflict.Count; i++){
                    if(faction == factionsInConflict[i]){
                        dne = false;
                    }
                    else if(faction.IsEnemy(factionsInConflict[i])){
                        return true;
                    }
                }
                if(dne){
                    factionsInConflict.Add(faction);
                }
            }
            return false;
        }
    }

    public Character[] Combatants{
        get {
            Character[] result = new Character[combatant_turn_info.Count];
            for(int i = 0; i < combatant_turn_info.Count; i++){
                result[i] = combatant_turn_info[i].Caster;
            }
            return result;
        }
    }

    #region next move info
    private bool flag;
    private CombatantInfo caster_info;
    #endregion


    // Start is called before the first frame update
    void Start(){
        combatant_turn_info = new List<CombatantInfo>();
        join_queue = new List<Character>();
        ready = true;
        StartCoroutine(ProcessBattle());
    }

    private IEnumerator ProcessBattle(){
        while(combatant_turn_info.Count < 2 || !HasConflict){
            AddCombatantsIntoFray();
            yield return new WaitForSeconds(0.1f);
        }
        ui_queue.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        // battle starts
        while(HasConflict){
            // add in other combatants
            AddCombatantsIntoFray();
            // update queue
            ui_queue.UpdateQueuePrediction();
            flag = false;
            // set next person
            caster_info = PopDelayQueue(combatant_turn_info);
            // start combatants turn
            StartCoroutine(caster_info.Caster.StartTurn());
            // wait for turn to be complete
            yield return WaitForFlag();
            // give a second to process
            yield return new WaitForSeconds(3.0f);
        }
        ui_queue.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public Character[] PredictTurns(int turns, Move next_move=null){
        if(turns < 1){
            return null;
        }
        // in case next_move is set to null
        if(next_move == null){ next_move = default_move; }

        Character[] playerOrder = new Character[turns];
        List<CombatantInfo> predict_delays = CopyDelayInfo(combatant_turn_info);
        // start prediction with first element
        CombatantInfo next_char_info = PopDelayQueue(predict_delays);
        playerOrder[0] = next_char_info.Caster;
        next_char_info.SetNewMove(next_move);
        // continue on with the rest of the player turns
        for(int i = 1; i < turns; i++){
            // find next person to run and update values
            next_char_info = PopDelayQueue(predict_delays);
            playerOrder[i] = next_char_info.Caster;
            // set delay to player
            next_char_info.SetNewMove(default_move);
        }
        return playerOrder;
    }

    public void MakeMove(Character moving_character, Move selected_move, Character target){
        // for now no move changes and if incorrect character submitting a move, ignore
        if(flag || moving_character != caster_info.Caster){ return; }
        // make move
        switch(selected_move.TargetRestriction){
            case TargetRestriction.ALL:
                foreach(CombatantInfo ci in combatant_turn_info){
                    Character c = ci.Caster;
                    if(c != null && c != moving_character){
                        ApplyMove(moving_character, selected_move, c);
                    }
                }
                break;
            case TargetRestriction.SELF:
                ApplyMove(moving_character, selected_move, moving_character);
                break;
            default:
                ApplyMove(moving_character, selected_move, target);
                break;
        }
        // set delay
        caster_info.SetNewMove(selected_move);
        //set flag so battle can proceed
        flag = true;
    }

    public void EnterBattle(Character c){
        if(c == null) { return; }
        // TODO: When turned to list, then add to queue to add into battle next turn
        join_queue.Add(c);
    }

    public void LeaveBattle(Character c){
        if(c == null) { return; }
        // TODO: When turned to list, then remove the character's info from list
        for(int i = 0; i < combatant_turn_info.Count; i++){
            if(combatant_turn_info[i].Caster == c){
                combatant_turn_info.RemoveAt(i);
                return;
            }
        }
    }

    private void AddCombatantsIntoFray(){
        while(join_queue.Count > 0){
            Character c = join_queue[0];
            join_queue.RemoveAt(0);
            c.Battle = this;
            combatant_turn_info.Add(new CombatantInfo(c, default_move));
        }
    }

    #region helper methods

    private void ApplyMove(Character casting_character, Move move, Character target){
        switch(move.Type){
            case MoveType.SUPPORT:
                target.Heal(move.Power);
                break;
            default:
                target.Damage(move.Power);
                break;
        }
    }

    private CombatantInfo PopDelayQueue(List<CombatantInfo> delayList){
        // find next combatant
        CombatantInfo combatant_info = null;
        for(int i = 0; i < delayList.Count; i++){
            // if on valid combattant
            if(delayList[i] != null && (combatant_info == null || combatant_info.CompareTo(delayList[i]) < 0)){
                combatant_info = delayList[i];
            }
        }
        // if there is a next one, reduce all in list by the delay of next combatant
        if(combatant_info != null){
            // hold remaining delay to remove from all in turn list
            int remaining_delay = combatant_info.DelayCurrent;
            foreach(CombatantInfo ci in delayList){
                if(ci != null){
                    // to avoid many at 0 delay
                    ci.ProgressTime(remaining_delay);
                }
            }
        }
        return combatant_info;
    }

    /**
     * copies from the array source into destination as much as possible, shallow copy
     * assumes the same length, will not copy if either of them are null
     */
    private List<CombatantInfo> CopyDelayInfo(List<CombatantInfo> source){
        if(source == null){
            return null;
        }
        List<CombatantInfo> result = new List<CombatantInfo>();
        foreach(CombatantInfo ci in source){
            result.Add(ci.Copy());
        }
        return result;
    }

    private IEnumerator WaitForFlag(){
        while(true){
            if(flag){
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion
}
