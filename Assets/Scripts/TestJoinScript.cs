using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJoinScript : MonoBehaviour
{
    [SerializeField]
    Character[] combatants;
    [SerializeField]
    Battle battlefield;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AddCombatants());
    }

    private IEnumerator AddCombatants(){
        while(!battlefield.IsReady){
            yield return new WaitForSeconds(0.1f);
        }
        foreach(Character c in combatants){
            battlefield.EnterBattle(c);
        }
        yield break;
    }
}
