using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonChoiceList : MonoBehaviour
{
    private enum Direction { FORWARD, BACKWARD };
    [SerializeField]
    private ButtonMiddleMan[] buttons;
    [SerializeField]
    private Button prev_button, next_button, back_button;

    [SerializeField]
    private GameObject button_preset;

    private UIChoice[] options;
    private int window_size;
    private int window_offset;

    private Player target;

    #region QueuePrediction related variables
    [SerializeField]
    private UIQueue queue;
    [SerializeField]
    private float delay_time = 0.1f;
    private float timer;
    private bool selection_changed;
    private Move next_move, possible_next;
    #endregion

    public UIChoice[] Options{
        get{ return options; }
        set{
            options = value;
            UpdateScreen();
        }
    }

    public Player Target{
        get { return target; }
        set {
            target = value;
        }
    }

    /** sets values to be changed, but wait a sec before changing to wait for more updates 
     *  due to the nature of how the button middle man script will set
     */
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
        window_size = buttons.Length;
        foreach(ButtonMiddleMan b in buttons){
            b.ParentList = this;
        }
        prev_button.interactable = false;
        next_button.interactable = false;
        selection_changed = false;
        timer = 0;
        next_move = null;
        possible_next = null;
    }

    void Update(){
        // reduce timer
        if(timer > 0){
            timer -= Time.deltaTime;
        }
        // else if the selection was changed
        else if(selection_changed){
            // update slection_changed
            selection_changed = false;
            timer = -1;
            // if actually something new
            if(possible_next != next_move){
                // set the new prediction
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

    private void ChangeOffset(Direction dir){
        // get the desired change
        int indexDelta = (dir == Direction.FORWARD)?1:-1;
        // if it is valid, udpate the options
        if(OffsetWithinBounds(window_offset + indexDelta)){
            window_offset += indexDelta;
            UpdateScreen();
        }
    }

    public void MakeChoice(UIChoice option){
        target.SendChoice(option);
    }

    // updates all options
    private void UpdateScreen(){
        if(options == null){
            foreach(ButtonMiddleMan b in buttons){
                b.Option = null;
            }
            prev_button.interactable = false;
            next_button.interactable = false;
            return;
        }
        int offset = window_offset * window_size;
        bool reachedEnd = false;
        // update buttons
        for(int i = 0; i < buttons.Length; i++){
            if(reachedEnd || offset + i >= options.Length){
                buttons[i].Option = null;
                reachedEnd = true;
            }
            else{
                buttons[i].Option = options[offset + i];
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
