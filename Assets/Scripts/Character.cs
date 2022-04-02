using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region stats
    // Unit Name
    [SerializeField]
    private string unit_name;
    // health
    [SerializeField]
    private int max_health = 20;
    [SerializeField]
    private int current_health;
    // speed 
    [SerializeField]
    private int speed = 20;
    public int Speed{
        get {
            return speed;
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

    #region events
    // health change update
    public delegate void OnHealthUpdate();
    public event OnHealthUpdate onHealthUpdate;

    #endregion

    void Start(){
        if(current_health == 0){
            current_health = max_health;
        }
    }

    public int GetModifiedDelay(int baseDelay){
        return baseDelay * 128 / speed;
    }

    public void Damage(int damage){
        current_health -= damage;
        if(current_health < 0) current_health = 0;
        onHealthUpdate();
    }
}
