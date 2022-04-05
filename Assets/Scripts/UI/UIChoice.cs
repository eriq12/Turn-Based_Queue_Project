using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChoiceType{ MOVE, TARGET, NONE };

public interface UIChoice
{
    public string Name{
        get;
    }
}
