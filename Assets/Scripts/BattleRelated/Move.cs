using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType {ATTACK, SUPPORT};
public enum TargetRestriction { ANY, ALL, ENEMY, ALLY, SELF };

[CreateAssetMenu(fileName = "MoveData", menuName = "ScriptableObjects/MoveData", order = 2)]
public class Move : ScriptableObject, UIChoice
{
    [SerializeField]
    private string move_name;
    [SerializeField]
    private int power;
    [SerializeField]
    private int delay;
    [SerializeField]
    private MoveType move_type;
    [SerializeField]
    private TargetRestriction target_restriction = TargetRestriction.ENEMY;

    public string Name{
        get { return move_name; }
    }

    public MoveType Type{
        get { return move_type; }
    }

    public TargetRestriction TargetRestriction {
        get { return target_restriction; }
    }

    public int Power{
        get { return power; }
    }

    public int Delay{
        get { return delay; }
    }
}
