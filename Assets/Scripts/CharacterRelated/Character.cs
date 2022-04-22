using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour, UIChoice
{
    #region stats
    // Unit Name
    [SerializeField]
    protected string unit_name;
    // health
    [SerializeField]
    private int max_health = 20;
    [SerializeField]
    private int current_health;
    // speed 
    [SerializeField]
    private int speed = 20;

    [SerializeField]
    protected Faction team;

    [SerializeField]
    protected Move[] moveset;

    #endregion

    #region accessors

    public int Speed{
        get {
            return speed;
        }
    }

    public Faction Faction{
        get {
            return team;
        }
    }

    public int Health{
        get {
            return current_health;
        }
    }

    public string Name{
        get {
            return unit_name;
        }
    }

    public int MaxHealth{
        get {
            return max_health;
        }
    }

    public bool IsAlive{
        get {
            return current_health > 0;
        }
    }

    public bool IsDead{
        get {
            return !IsAlive;
        }
    }

    public Battle Battle{
        get { return current_battle; }
        set { current_battle = value; }
    }

    #endregion

    #region turn choice related
    // ui to use
    [SerializeField]
    protected ButtonChoiceList ui_options;
    // move selected
    protected Move turn_move;
    protected Character target;
    // what are we choosing right now?
    protected ChoiceType choice_type = ChoiceType.NONE;
    protected bool choice_flag = false;
    #endregion

    protected Battle current_battle;

    #region events
    // health change update
    public delegate void OnHealthUpdate();
    public event OnHealthUpdate onHealthUpdate;

    #endregion

    void Start(){
        if(current_health == 0){
            current_health = max_health;
        }
        current_battle = null;
    }

    public int GetModifiedDelay(int baseDelay){
        return baseDelay * 128 / speed;
    }

    public bool Damage(int damage){
        current_health -= damage;
        if(current_health < 0){
            current_health = 0;
        }
        onHealthUpdate();
        return IsAlive;
    }

    public void Heal(int restore){
        current_health += restore;
        if(current_health > max_health){
            current_health = max_health;
        }
        onHealthUpdate();
    }

    public bool ValidTarget(Character target, Move move = null){
        if(move == null){
            // this should never happen, but in case return false
            if(turn_move == null) { return false; }
            move = turn_move;
        }
        // also should never happen but in case return false
        if(target == null && 
            !(move.TargetRestriction == TargetRestriction.ALL || 
                move.TargetRestriction == TargetRestriction.ALLY)){
            return false;
        }
        // copying over from the other branch
        // return false if the following conditions are true:
        // can only target enemies and the target is an ally
        // can only target allies and the target is an enemy
        // other options should automatically target or have no restrictions
        if(move.TargetRestriction == TargetRestriction.ENEMY && Faction.IsAlly(target.Faction) ||
            move.TargetRestriction == TargetRestriction.ALLY && Faction.IsEnemy(target.Faction)){
            return false;
        }
        return true;

    }

    public abstract IEnumerator StartTurn();
}
