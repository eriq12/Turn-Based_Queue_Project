using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Non_Player_Character : Character
{
    public override IEnumerator StartTurn(){
        yield return new WaitForSeconds(0.5f);
        // search through the battle field for the first enemy
        Character[] targets = current_battle.Combatants;
        foreach(Character c in targets){
            if(c.IsAlive && team.IsEnemy(c.Faction)){
                current_battle.MakeMove(this, moveset[0], c);
                yield break;
            }
        }
    }
}
