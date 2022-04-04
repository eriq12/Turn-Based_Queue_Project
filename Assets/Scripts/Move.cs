using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType {Attack, Support};

[CreateAssetMenu(fileName = "MoveData", menuName = "ScriptableObjects/MoveData", order = 2)]
public class Move : ScriptableObject
{
    [SerializeField]
    private string move_name;
    [SerializeField]
    private int power;
    [SerializeField]
    private int delay;
    [SerializeField]
    private MoveType move_type;

    public string Name{
        get { return move_name; }
    }

    public MoveType Type{
        get { return move_type; }
    }

    public int Power{
        get { return power; }
    }

    public int Delay{
        get { return delay; }
    }
}
