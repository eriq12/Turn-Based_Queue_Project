using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FactionData", menuName = "ScriptableObjects/FactionData", order = 1)]
public class Faction : ScriptableObject
{
    [SerializeField]
    private string faction_name;
    [SerializeField]
    private Faction[] enemies;
    [SerializeField]
    private Faction[] allies;

    public string Name{
        get { return faction_name; }
    }

    public bool IsEnemy(Faction other){
        return ArrayContains(other, enemies);
    }

    public bool IsAlly(Faction other){
        return other == this || ArrayContains(other, allies);
    }

    private bool ArrayContains(Faction other, Faction[] arr){
        foreach(Faction f in arr){
            if(other == f){
                return true;
            }
        }
        return false;
    }
}
