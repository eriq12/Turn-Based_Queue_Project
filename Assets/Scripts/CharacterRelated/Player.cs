using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    #region turn choice related
    // ui to use
    [SerializeField]
    private ButtonChoiceList ui_options;
    // move selected
    private Move turn_move;
    private Character target;
    // what are we choosing right now?
    private ChoiceType choice_type = ChoiceType.NONE;
    private bool choice_flag = false;
    #endregion

    public override IEnumerator StartTurn(){
        // clear previous selections
        turn_move = null;
        target = null;
        ui_options.Target = this;
        // start up selections
        choice_type = ChoiceType.MOVE;
        ui_options.Options = moveset;
        yield return WaitForChoice();
        choice_type = ChoiceType.TARGET;
        ui_options.Options = current_battle.Combatants;
        yield return WaitForChoice();
        ui_options.Options = null;
        current_battle.MakeMove(this, turn_move, target);
        yield break;
    }

    private IEnumerator WaitForChoice(){
        while(true){
            if(choice_flag){
                choice_flag = false;
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SendChoice(UIChoice choice){
        if(choice_type == ChoiceType.MOVE && choice is Move){
            choice_type = ChoiceType.NONE;
            turn_move = (Move)choice;
            choice_flag = true;
        }
        else if(choice_type == ChoiceType.TARGET && choice is Character){
            choice_type = ChoiceType.NONE;
            target = (Character)choice;
            choice_flag = true;
        }
    }
}
