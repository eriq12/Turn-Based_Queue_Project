using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UISettings", menuName = "ScriptableObjects/UISettings", order = 3)]
public class UISetting : ScriptableObject
{
    public int option_row_amount;
    public int option_col_amount;
    public int option_spacing;
    public int option_width;
}
