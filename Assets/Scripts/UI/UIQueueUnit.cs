using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQueueUnit : MonoBehaviour
{
    [SerializeField]
    private Image health_bar_foreground;
    [SerializeField]
    private Image health_bar_animation;

    [SerializeField]
    private Text unit_name;

    [SerializeField]
    private Text health_info;

    [SerializeField]
    private Character target_unit;

    // essentially how fast should the health bar deplete (in terms of percentage of the bar)
    private static float animation_rate = 0.5f;

    void Start(){
        health_bar_foreground.fillAmount = 0;
        health_bar_animation.fillAmount = 0;
        health_info.text = "0/0";
        unit_name.text = "PH";
    }

    void Update(){
        // to decrease by specified animation rate until it passes or reaches the actual health value
        if(health_bar_animation.fillAmount > health_bar_foreground.fillAmount){
            health_bar_animation.fillAmount -= animation_rate * Time.deltaTime;
        }
        // to clamp
        if(health_bar_animation.fillAmount < health_bar_foreground.fillAmount){
            health_bar_animation.fillAmount = health_bar_foreground.fillAmount;
        }
    }

    public void UpdateUnitTarget(Character new_unit){
        // if it asks to update to the same one, then just leave
        if(target_unit == new_unit){
            return;
        }
        // remove subscription if needed
        if(target_unit != null){
            target_unit.onHealthUpdate -= UpdateHealth;
        }
        // subscribe to new target's event
        new_unit.onHealthUpdate += UpdateHealth;
        // update unit name
        unit_name.text = new_unit.Name;

        // update both foreground and animation
        float new_fill = ((float)new_unit.Health)/new_unit.MaxHealth;
        health_bar_foreground.fillAmount = new_fill;
        health_bar_animation.fillAmount = new_fill;
        // update health stats
        health_info.text = $"{new_unit.Health}/{new_unit.MaxHealth}";
        target_unit = new_unit;
    }

    private void UpdateHealth(){
        health_bar_foreground.fillAmount = ((float)target_unit.Health)/target_unit.MaxHealth;
        health_info.text = $"{target_unit.Health}/{target_unit.MaxHealth}";
    }
}
