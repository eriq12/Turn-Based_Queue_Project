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
    [SerializeField]
    private Move default_move;

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

    /// <summary>
    /// The coroutine which handles the battle loop and maintains the turns for each of the combating characters.
    /// </summary>
    private IEnumerator StartBattle(){
        // makes sure the UI queue is active
        ui_queue.gameObject.SetActive(true);
        // I wait here in case the parts of the UI queue are not initialized
        yield return new WaitForSeconds(0.1f);
        // TODO: maybe wait for a signal before starting a battle, to allow combatants to join the fray?
        // battle starts
        while(true){
            // TODO: ideally check for win/end conditions here
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

    /// <summary>
    /// Get the character at a specified index.
    /// </summary>
    /// <param name="index">The index of the character to get</param>
    /// <returns>The character at the specified index, if not in range then returns null.</returns>
    public Character GetCharacter(int index){
        return (index < num_combatants && index >= 0)?combatants[num_combatants]:null;
    }

    /// <summary>
    /// This method will predict the next specified amount of turns.
    /// </summary>
    /// <param name="turns">The number of turns into the battle to predict character order.</param>
    /// <param name="next_move">(Optional)The predicted turn of the next character to move.</param>
    /// <returns>The turn prediction as an array of Characters.</returns>
    public Character[] PredictTurns(int turns, Move next_move=null){
        // protection in case the amount of turns asked is 0 or less
        if(turns < 1) { return null; }
        // in case if there is no next move
        if(next_move == null) { next_move = default_move; }
        Character[] playerOrder = new Character[turns];
        int[] predictDelays = new int[delays.Length];
        CopyArrays<int>(delays, predictDelays);
        // do first prediction in case of a specified next_move
        int currentTempPos = PopDelayQueue(predictDelays);
        playerOrder[0] = combatants[currentTempPos];
        predictDelays[currentTempPos] = combatants[currentTempPos].GetModifiedDelay(next_move.Delay);
        // start remaining predictions
        for(int i = 1; i < turns; i++){
            // find next person to run and update values
            currentTempPos = PopDelayQueue(predictDelays);
            playerOrder[i] = combatants[currentTempPos];
            // set delay to player
            predictDelays[currentTempPos] = combatants[currentTempPos].GetModifiedDelay(default_move.Delay);
        }
        return playerOrder;
    }

    /// <summary>
    /// This method should be called whenever a character wants to make a move/perform their turn
    /// </summary>
    /// <param name="moving_character">The character from where this method should be invoked/called.</param>
    /// <param name="selected_move">The move that the character whishes to perform.</param>
    /// <param name="target">The character to which the move will be targeting/afflicting.</param>
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
    /// <summary>
    /// Gets the index of the next character to move, and progresses the time for all characters in battle up to the next character's turn.
    /// </summary>
    /// <param name="delayList">The array of delays, MUST be associated with the combatants array of the same battle instance to operate correctly.</param>
    /// <returns>The index of the next character to move.</returns>
    private int PopDelayQueue(int[] delayList){
        //// this method has two parts: finding the next guy, then advancing time to the earliest person's time

        //// part 1: find next combatant
        int combatant = -1;
        // loop through the delay list to look for a combatant
        // with less delay (would move sooner than currently referenced)
        for(int i = 0; i < delayList.Length; i++){
            // if on valid combattant and sooner delay 
            // (the delay of current index has less than the referenced index)
            if(delayList[i] != -1 && (combatant == -1 || delayList[combatant] > delayList[i])){
                // change to reference current point
                combatant = i;
            }
        }

        //// part 2: reduce all by the delay of the earliest moving combatant
        int delay_to_pass = delayList[combatant];
        // if there is a next one, reduce all in list by the delay of next combatant
        if(combatant != -1){
            for(int i = 0; i < delayList.Length; i++){
                if(delayList[i] != -1){
                    // to avoid many at 0 delay
                    delayList[i] -= delay_to_pass;
                }
            }
        }

        //// finally return the index of who is going next
        return combatant;
    }

    /// <summary>
    /// Copies from the array source into destination as much as possible, shallow copy
    /// assumes the same length, will not copy if either of them are null
    /// </summary>
    /// <param name="source">The source array to copy values from.</param>
    /// <param name="destination">The array to copy values to.</param>
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
