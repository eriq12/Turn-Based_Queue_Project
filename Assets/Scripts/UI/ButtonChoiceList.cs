using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonChoiceList : MonoBehaviour
{
    private enum Direction { FORWARD, BACKWARD };
    [SerializeField]
    private GameObject selection_page;
    [SerializeField]
    private ButtonMiddleMan[] buttons;
    [SerializeField]
    private Button prev_button, next_button, back_button;

    [SerializeField]
    private GameObject button_preset;

    private UIChoice[] options;
    private int window_size;
    private int window_offset;

    [SerializeField]
    private UIQueue queue;
    [SerializeField]
    private float delay_time = 0.1f;
    private float timer;
    private bool selection_changed;

    private int buttons_initialized;

    private Move next_move, possible_next;

    private Player target;

    public UIChoice[] Options{
        get{ return options; }
        set{
            Debug.Log("Options of the Options UI has been changed.");
            options = value;
            StartCoroutine(UpdateScreen());
        }
    }

    public Player Target{
        get { return target; }
        set {
            target = value;
        }
    }

    // tries to make changes not immediately change the ui, 
    // as there are events on select and deselect /
    // on pointer enter and exit, 
    // so it waits a split sec before changing, should it actually be changed
    public Move SelectedMove{
        get { return next_move; }
        set {
            if(possible_next != value){
                possible_next = value;
                timer = delay_time;
                selection_changed = true;
            }
        }
    }
    
    void Start(){
        buttons_initialized = 0;
        window_size = buttons.Length;
        foreach(ButtonMiddleMan b in buttons){
            b.ParentList = this;
        }
        prev_button.interactable = false;
        next_button.interactable = false;
        selection_changed = false;
        timer = 0;
    }

    // this is designed to limit the amount of times the UpdateQueuePrediction method is called
    void Update(){
        if(timer > 0){
            timer -= Time.deltaTime;
        }
        else if(selection_changed){
            selection_changed = false;
            timer = -1;
            if(possible_next != next_move){
                queue.UpdateQueuePrediction(possible_next);
                next_move = possible_next;
            }
        }
    }

    public void NextPage(){
        ChangeOffset(Direction.FORWARD);
    }

    public void PrevPage(){
        ChangeOffset(Direction.BACKWARD);
    }

    public void MarkInitialized(){
        buttons_initialized++;
        if(buttons_initialized == buttons.Length){
            selection_page.SetActive(false);
            Debug.Log("All buttons have been initialized, deactivating selection page.");
        }
    }

    private void ChangeOffset(Direction dir){
        // get the desired change
        int indexDelta = (dir == Direction.FORWARD)?1:-1;
        // if it is valid, udpate the options
        if(OffsetWithinBounds(window_offset + indexDelta)){
            window_offset += indexDelta;
            StartCoroutine(UpdateScreen());
        }
    }

    public void MakeChoice(UIChoice option){
        target.SendChoice(option);
    }

    // updates all options
    private IEnumerator UpdateScreen(){
        // wait 
        if(options == null && selection_page.activeInHierarchy){
            foreach(ButtonMiddleMan b in buttons){
                b.Option = null;
                b.interactable = false;
            }
            prev_button.interactable = false;
            next_button.interactable = false;
            selection_page.SetActive(false);
            yield break;
        }
        if(!selection_page.activeInHierarchy){
            selection_page.SetActive(true);
            yield return null;
        }
        int offset = window_offset * window_size;
        bool reachedEnd = false;
        // update buttons
        for(int i = 0; i < buttons.Length; i++){
            if(reachedEnd || offset + i >= options.Length){
                buttons[i].Option = null;
                buttons[i].interactable = false;
                reachedEnd = true;
            }
            else{
                buttons[i].Option = options[offset + i];
                buttons[i].interactable = (options[offset + i] is Move) || target.ValidTarget((Character)options[offset + i]);
            }
        }
        // update next and prev
        next_button.interactable = !reachedEnd;
        prev_button.interactable = window_offset > 0;
    }

    private bool OffsetWithinBounds(int new_index){
        return new_index * window_size < options.Length && new_index >= 0;
    }
}
