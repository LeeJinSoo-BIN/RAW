using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterState : MonoBehaviour
{
    public int maxHealth = 1000;
    public int maxMana = 1000;
    public float power = 10f;
    private Slider health;
    private Slider shield;
    public float criticalPercent = 50f;
    public float criticalDamage = 1.2f;
    public Animator characterAnimator;
    public float healPercent = 1f;
    private bool isDeath = false;

    private Dictionary<string, int> skillLevel = new Dictionary<string, int>();
    void Start()
    {
        health = transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        shield = transform.GetChild(0).GetChild(2).GetComponent<Slider>();
        characterAnimator = transform.parent.GetComponentInChildren<Animator>();
        health.maxValue = maxHealth;
        health.value = maxHealth;
        shield.maxValue = maxHealth;
        shield.value = 0;

        skillLevel.Add("magic_floor", 1);
        skillLevel.Add("magic_totem", 1);
        skillLevel.Add("magic_heal", 1);
        skillLevel.Add("magic_global_heal", 1);
        skillLevel.Add("arrow_charge", 1);
        skillLevel.Add("arrow_rain", 1);
        skillLevel.Add("arrow_gatling", 1);
        skillLevel.Add("sword_smash", 1);
        skillLevel.Add("sword_shield", 1);
        skillLevel.Add("sword_slash", 1);
        skillLevel.Add("sword_bind", 1);



    }

    // Update is called once per frame
    
    public void ProcessSkill(int type, float value)
    {        
        if (type == 0 || type == 4) // damage
        {            
            float _shield = shield.value;
            shield.value -= value;
            value -= _shield;
            Debug.Log(value);            
            if (value > 0)
                health.value -= value;
            if (health.value <= 0 && !isDeath)
            {
                isDeath = true;
                characterAnimator.SetTrigger("Death");
                characterAnimator.SetBool("IsDeath", true);
            }
        }
        else if(type == 1) //heal
        {
            health.value += value * healPercent;
        }
        else if (type == 2)//shield
        {
            shield.value += value;
        }
        else if (type == 3) // power
        {
            power += value;
        }
    }
   
}
