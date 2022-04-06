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
    
    void Start(){
        window_size = buttons.Length;
        foreach(ButtonMiddleMan b in buttons){
            b.ParentList = this;
        }
        prev_button.interactable = false;
        next_button.interactable = false;
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
