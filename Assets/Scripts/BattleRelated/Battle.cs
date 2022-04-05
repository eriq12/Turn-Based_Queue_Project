using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    private static int num_combatants = 10;
    [SerializeField]
    private UIQueue ui_queue;

    [SerializeField]
    private Character[] combatants;
    private int[] delays;
    private int turnState;

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
        delays = new int[combatants.Length];
        // assume all joining combatants are alive
        for(int i = 0; i < combatants.Length; i++){
            combatants[i].EnterBattle(this);
            delays[i] = (combatants[i]==null)?-1:combatants[i].GetModifiedDelay(100);
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
            caster_index = PopDelayQueue(delays);
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
        Character[] playerOrder = new Character[turns];
        int[] predictDelays = new int[delays.Length];
        CopyArrays<int>(delays, predictDelays);
        // start prediction
        int currentTempPos = -1;
        for(int i = 0; i < turns; i++){
            // find next person to run and update values
            currentTempPos = PopDelayQueue(predictDelays);
            playerOrder[i] = combatants[currentTempPos];
            // set delay to player
            predictDelays[currentTempPos] = combatants[currentTempPos].GetModifiedDelay(100);
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
        delays[caster_index] = caster.GetModifiedDelay(selected_move.Delay);
        //set flag so battle can proceed
        flag = true;
    }

    #region helper methods
    private int PopDelayQueue(int[] delayList){
        // find next combatant
        int combatant = -1;
        for(int i = 0; i < delayList.Length; i++){
            // if on valid combattant
            if(delayList[i] != -1 && (combatant == -1 || delayList[combatant] > delayList[i])){
                combatant = i;
            }
        }
        // if there is a next one, reduce all in list by the delay of next combatant
        if(combatant != -1){
            for(int i = 0; i < delayList.Length; i++){
                if(delayList[i] != -1){
                    // to avoid many at 0 delay
                    delayList[i] = (delayList[i] == delayList[combatant])?1:delayList[i]-delayList[combatant];
                }
            }
        }
        return combatant;
    }

    /**
     * copies from the array source into destination as much as possible, shallow copy
     * assumes the same length, will not copy if either of them are null
     */
    private void CopyArrays<T>(T[] source, T[] destination){
        if(source == null || destination == null){
            return;
        }
        for(int i = 0; i < destination.Length; i++){
            destination[i] = source[i];
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
