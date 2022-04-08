using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#region queue_datatypes
public class TurnInfo : IComparable<TurnInfo>{
    #region information
    private Character caster;
    private int caster_index;
    private int delay_set;
    private int delay_left;
    private int turn;
    #endregion

    #region accessors
    public Character Caster{
        get{ return caster; }
    }
    public int CasterIndex{
        get{ return caster_index; }
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
    #endregion

    public TurnInfo(Character c, int indx, Move move){
        caster = c;
        caster_index = indx;
        SetNewMove(move);
    }

    private TurnInfo(Character c, int indx, int dNow, int dOrg){
        caster = c;
        caster_index = indx;
        delay_left = dNow;
        delay_set = dOrg;
    }

    public void SetNewMove(Move move){
        delay_set = caster.GetModifiedDelay(move.Delay);
        delay_left = delay_set;
        turn++;
    }

    public TurnInfo Copy(){
        return new TurnInfo(caster, caster_index, delay_left, delay_set);
    }

    public void ProgressTime(int delay){
        delay_left -= delay;
    }

    public int CompareTo(TurnInfo other){
        // first check delay left
        int result = other.DelayCurrent - delay_left ;
        if(result != 0){ return result; }
        // then check turn
        result = other.Turn - turn;
        if(result != 0){ return result; }
        // then check for who had the longer wait
        result = other.DelayOriginal - delay_set;
        if(result != 0){ return result; }
        // finally just do by index
        return other.CasterIndex - caster_index;
    }
}
#endregion

public class Battle : MonoBehaviour
{
    private static int num_combatants = 10;
    [SerializeField]
    private UIQueue ui_queue;

    [SerializeField]
    private Character[] combatants;
    private TurnInfo[] combatant_turn_info;
    private int turnState;
    [SerializeField]
    private Move default_move;

    public Move DefaultMove{
        get{ return default_move; }
    }

    public Character[] Combatants{
        get { return combatants; }
    }

    #region next move info
    private bool flag;
    private Character caster;
    private int caster_index;
    #endregion


    // Start is called before the first frame update
    void Start(){
        if(combatants == null){
            combatants = new Character[num_combatants];
        }
        combatant_turn_info = new TurnInfo[combatants.Length];
        // assume all joining combatants are alive
        for(int i = 0; i < combatants.Length; i++){
            combatants[i].EnterBattle(this);
            combatant_turn_info[i] = new TurnInfo(combatants[i], i, default_move);
        }
        StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle(){
        ui_queue.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        // battle starts
        while(true){
            ui_queue.UpdateQueuePrediction();
            flag = false;
            // set next person
            caster_index = PopDelayQueue(combatant_turn_info);
            caster = combatants[caster_index];
            // start combatants turn
            StartCoroutine(caster.StartTurn());
            // wait for turn to be complete
            yield return WaitForFlag();
            // give a second to process
            yield return new WaitForSeconds(3.0f);
        }
    }

    public Character GetCharacter(int index){
        return (index < num_combatants)?combatants[num_combatants]:null;
    }

    public Character[] PredictTurns(int turns){
        return PredictTurns(turns, default_move);
    }

    public Character[] PredictTurns(int turns, Move next_move){
        if(turns < 1){
            return null;
        }
        Character[] playerOrder = new Character[turns];
        TurnInfo[] predict_delays = new TurnInfo[combatant_turn_info.Length];
        CopyDelayInfo(combatant_turn_info, predict_delays);
        // start prediction with first element
        int currentTempPos = PopDelayQueue(predict_delays);
        playerOrder[0] = combatants[currentTempPos];
        predict_delays[currentTempPos].SetNewMove(next_move);
        // continue on with the rest of the player turns
        for(int i = 1; i < turns; i++){
            // find next person to run and update values
            currentTempPos = PopDelayQueue(predict_delays);
            playerOrder[i] = combatants[currentTempPos];
            // set delay to player
            predict_delays[currentTempPos].SetNewMove(default_move);
        }
        return playerOrder;
    }

    public void MakeMove(Character moving_character, Move selected_move, Character target){
        // for now no move changes and if incorrect character submitting a move, ignore
        if(flag || moving_character != caster){ return; }
        // make move
        if(selected_move.Type == MoveType.ATTACK){
            target.Damage(selected_move.Power);
        }
        else if(selected_move.Type == MoveType.SUPPORT){
            target.Heal(selected_move.Power);
        }
        // set delay
        combatant_turn_info[caster_index].SetNewMove(selected_move);
        //set flag so battle can proceed
        flag = true;
    }

    #region helper methods
    private int PopDelayQueue(TurnInfo[] delayList){
        // find next combatant
        TurnInfo combatant = null;
        for(int i = 0; i < delayList.Length; i++){
            // if on valid combattant
            if(delayList[i] != null && (combatant == null || combatant.CompareTo(delayList[i]) < 0)){
                combatant = delayList[i];
            }
        }
        if(combatant == null){
            return -1;
        }
        // hold remaining delay to remove from all in turn list
        int remaining_delay = combatant.DelayCurrent;
        // if there is a next one, reduce all in list by the delay of next combatant
        if(combatant != null){
            for(int i = 0; i < delayList.Length; i++){
                if(delayList[i] != null){
                    // to avoid many at 0 delay
                    delayList[i].ProgressTime(remaining_delay);
                }
            }
        }
        return combatant.CasterIndex;
    }

    /**
     * copies from the array source into destination as much as possible, shallow copy
     * assumes the same length, will not copy if either of them are null
     */
    private void CopyDelayInfo(TurnInfo[] source, TurnInfo[] destination){
        if(source == null || destination == null){
            return;
        }
        for(int i = 0; i < destination.Length; i++){
            destination[i] = source[i].Copy();
        }
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
