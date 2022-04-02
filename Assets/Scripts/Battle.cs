using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    private static int num_combattants = 10;
    [SerializeField]
    private Character[] combattants;
    private int[] delays;
    private int turnState;
    [SerializeField]
    private UIQueue ui_queue;


    // Start is called before the first frame update
    void Start()
    {
        if(combattants == null){
            combattants = new Character[num_combattants];
        }
        delays = new int[combattants.Length];
        // assume all joining combatants are alive
        for(int i = 0; i < combattants.Length; i++){
            delays[i] = (combattants[i]==null)?-1:combattants[i].GetModifiedDelay(100);
        }
        StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle(){
        ui_queue.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        // 
        ui_queue.UpdateQueuePrediction();
    }

    public Character GetCharacter(int index){
        return (index < num_combattants)?combattants[num_combattants]:null;
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
            playerOrder[i] = combattants[currentTempPos];
            // set delay to player
            predictDelays[currentTempPos] = combattants[currentTempPos].GetModifiedDelay(100);
        }
        return playerOrder;
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
    #endregion
}
