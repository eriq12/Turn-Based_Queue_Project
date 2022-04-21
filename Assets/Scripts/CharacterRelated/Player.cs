using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public override IEnumerator StartTurn(){
        // clear previous selections
        Debug.Log(gameObject.name + "'s turn has begun, clearing fields to begin turn...");
        turn_move = null;
        target = null;
        Debug.Log("Setting UI options page's target to " + gameObject.name);
        ui_options.Target = this;
        // start up selections
        choice_type = ChoiceType.MOVE;
        ui_options.Options = moveset;
        yield return WaitForChoice();
        // should the types not need targeting, just continue
        if(turn_move.TargetRestriction == TargetRestriction.ALL ||
            turn_move.TargetRestriction == TargetRestriction.SELF){
            ui_options.Options = null;
            current_battle.MakeMove(this, turn_move, null);
        }
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
