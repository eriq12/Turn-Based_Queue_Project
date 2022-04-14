using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonMiddleMan : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
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

    #region selection event handlers
    /// <summary>
    /// When the button is selected, it envokes the method 
    /// UpdateSelection <see cref="UpdateSelection"/> 
    /// to change prediction queue (if the option is a move).
    /// Selection counter part of OnPointerEnter <see cref="OnPointerEnter"/>
    /// </summary>
    public void OnSelect(BaseEventData eventData){
        UpdateSelection();
    }
    public void OnDeselect(BaseEventData eventData){
        UpdateSelection(false);
    }
    /// <summary>
    /// When the button is moused over, it envokes the method 
    /// UpdateSelection <see cref="UpdateSelection"/> 
    /// to change prediction queue (if the option is a move).
    /// Pointer counter part of OnSelect <see cref="OnSelect"/>
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData){
        UpdateSelection();
    }
    public void OnPointerExit(PointerEventData eventData){
        UpdateSelection(false);
    }
    private void UpdateSelection(bool selecting=true){
        if(option is Move){
            parent_list.SelectedMove = (selecting)?(Move)option:null;
        }
    }
    #endregion
}
