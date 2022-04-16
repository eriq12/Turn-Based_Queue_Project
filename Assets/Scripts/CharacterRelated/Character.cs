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
    protected int max_health = 20;
    [SerializeField]
    protected int current_health;
    // speed 
    [SerializeField]
    protected int speed = 20;

    [SerializeField]
    protected Faction team;

    [SerializeField]
    protected Move[] moveset;

    #endregion

     #region turn choice related
    // ui to use
    [SerializeField]
    protected ButtonChoiceList ui_options;

    // move selected
    protected Move turn_move;

    // the target of the move (who are we performing the move on)
    protected Character target;

    // what are we choosing right now?
    protected ChoiceType choice_type = ChoiceType.NONE;

    // used in the WaitForChoice calls
    protected bool choice_flag = false;
    #endregion

    #region accessors

    public string Name{
        get {
            return unit_name;
        }
    }

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

    /// <summary>
    /// Method <c>GetModifiedDelay</c> takes in the base delay and returns speed modified delay
    /// </summary>
    /// <param name="baseDelay">the base delay of the move that the method takes</param>
    public int GetModifiedDelay(int baseDelay){
        return baseDelay * 128 / speed;
    }

    public void Damage(int damage){
        current_health -= damage;
        if(current_health < 0){
            current_health = 0;
        }
        onHealthUpdate();
    }

    public void Heal(int restore){
        current_health += restore;
        if(current_health > max_health){
            current_health = max_health;
        }
        onHealthUpdate();
    }

    public void EnterBattle(Battle b){
        current_battle = b;
    }

    public bool ValidTarget(Character target, Move move = null){
        if(move == null){
            // should never happen, but in case, just return false
            if(turn_move == null) { return false; }
            move = turn_move;
        }
        // technically under the code for selecting in UI, 
        // this should also never come here, but in case it should also return null
        if(target == null && !(move.TargetRestriction == TargetRestriction.ALL || move.TargetRestriction == TargetRestriction.ALLY)){
            return false;
        }
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

    public void LeaveBattle(){
        current_battle = null;
    }

    /// <summary>
    /// This method is invoked everytime the character can move.
    /// To decide on the turn the character wants to take, the
    /// instance needs to invoke the battle's MakeMove method.
    /// </summary>
    public abstract IEnumerator StartTurn();
}
