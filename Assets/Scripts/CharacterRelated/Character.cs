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

    public void LeaveBattle(){
        current_battle = null;
    }

    public abstract IEnumerator StartTurn();
}
