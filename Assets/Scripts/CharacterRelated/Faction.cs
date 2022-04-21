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
        return System.Array.IndexOf(enemies, other) != -1;
    }

    public bool IsAlly(Faction other){
        return other == this || System.Array.IndexOf(allies, other) != -1;
    }
}
