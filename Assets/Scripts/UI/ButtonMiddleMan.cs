using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMiddleMan : MonoBehaviour
{
    private UIChoice option;
    private Button button;
    private Text text;

    private ButtonChoiceList parent_list;

    public UIChoice Option{
        get{ return option; }
        set{
            if(value == option){
                return;
            }
            else if(value == null){
                button.interactable = false;
                text.text = "";
                option = null;
            }
            else{
                button.interactable = true;
                text.text = value.Name;
                option = value;
            }
        }
    }

    public ButtonChoiceList ParentList{
        get{ return parent_list; }
        set{ parent_list = value; }
    }

    void Start(){
        // if the button and text aren't set, then find them
        if(button == null){
            button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(ChooseOption);
        }
        if(text == null){
            text = gameObject.GetComponentInChildren<Text>();
        }
        // make sure option is null and button is disabled and text is empty
        button.interactable = false;
        text.text = "";
        option = null;
    }

    public void ChooseOption(){
        parent_list.MakeChoice(option);
    }
}
