using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{

    public override IEnumerator StartTurn(){
        // clear previous selections
        turn_move = null;
        target = null;
        ui_options.Target = this;
        // start up selections
        choice_type = ChoiceType.MOVE;
        ui_options.Options = moveset;
        yield return WaitForChoice();
        // if applies to all or only self, then no need to select a target
        if(turn_move.TargetRestriction == TargetRestriction.ALL || turn_move.TargetRestriction == TargetRestriction.SELF){
            ui_options.Options = null;
            current_battle.MakeMove(this, turn_move, null);
            yield break;
        }
        choice_type = ChoiceType.TARGET;
        ui_options.Options = current_battle.Combatants;
        yield return WaitForChoice();
        ui_options.Options = null;
        current_battle.MakeMove(this, turn_move, target);
        yield break;
    }

    /// <summary> 
    /// A method specific to the player class, so the 
    /// StartTurn coroutine can wait for decisions to be
    /// made before continuing on.
    /// </summary>
    private IEnumerator WaitForChoice(){
        // repeat to infinity
        while(true){
            // if flag raised, enter
            if(choice_flag){
                // lower the flag
                choice_flag = false;
                // break out of the loop/coroutine
                yield break;
            }
            // wait to avoid having it repeat excessively
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary> 
    /// Allows the UI to submit choices as the player.
    /// </summary>
    public void SendChoice(UIChoice choice){
        // checks if the submitted choice is a Move when you want it
        if(choice_type == ChoiceType.MOVE && choice is Move){
            choice_type = ChoiceType.NONE;
            // set the choice
            turn_move = (Move)choice;
            // raise the flag to stop waiting
            choice_flag = true;
        }
        // checks if the submitted choice is a Character when you want to select a target
        else if(choice_type == ChoiceType.TARGET && choice is Character){
            choice_type = ChoiceType.NONE;
            // set the choice
            target = (Character)choice;
            // raise the flag to stop waiting
            choice_flag = true;
        }
    }
}
