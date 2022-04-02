using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQueue : MonoBehaviour
{
    // singleton class
    private GameObject uiqueue;
    [SerializeField]
    private int queue_length;

    [SerializeField]
    private GameObject queue_unit_prefab;

    private UIQueueUnit[] queue_array;

    [SerializeField]
    private Battle battle;

    public int Length{
        get{
            return queue_array.Length;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(uiqueue != null){
            Destroy(this.gameObject);
        }
        uiqueue = this.gameObject;
        if(queue_array == null){
            queue_array = new UIQueueUnit[queue_length];
        }
        for(int i = 0; i < queue_array.Length; i++){
            if(queue_array[i] == null){
                GameObject temp = Instantiate(queue_unit_prefab, gameObject.transform);
                queue_array[i] = temp.GetComponent<UIQueueUnit>();
                RectTransform tempui = temp.GetComponent<RectTransform>();
                Vector2 temppos = tempui.anchoredPosition;
                tempui.anchoredPosition = new Vector2(temppos.x, 20 + 170 * i);
            }
        }
    }

    // updates battlefield
    public void SetBattle(Battle new_battle){
        battle = new_battle;
    }

    public void UpdateQueuePrediction(){
        Character[] list = battle.PredictTurns(queue_array.Length);
        for(int i = 0; i < queue_array.Length; i++){
            queue_array[i].UpdateUnitTarget(list[i]);
        }
    }
}
