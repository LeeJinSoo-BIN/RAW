using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterState : MonoBehaviour
{
    public int maxHealth = 1000;
    public int maxMana = 1000;
    public int power = 10;    
    private Slider health;
    private Slider shield;
    public float criticalPercet = 50;
    public float criticalDamage = 1.2f;
    public Animator characterAnimator;
    public float healPercent = 1f;
    void Start()
    {
        health = transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        shield = transform.GetChild(0).GetChild(2).GetComponent<Slider>();
        characterAnimator = transform.GetChild(0).GetComponent<Animator>();
        health.maxValue = maxHealth;
        health.value = maxHealth;
        shield.maxValue = maxHealth;
        shield.value = 0;
    }

    // Update is called once per frame
    public void Damage(float damagePercent)
    {
        float crit = Random.Range(0f, 100f);
        
        float critical_damage = criticalDamage;
        if (crit < criticalPercet)
            critical_damage = 1f;
        int damage = (int)((damagePercent / 100 * power * critical_damage));

        float _shield = shield.value;
        shield.value -= damage;
        damage -= (int)_shield;
        if(damage > 0)
            health.value -= damage;

        if (health.value <= 0)
        {
            characterAnimator.SetBool("isDeath", true);
            characterAnimator.SetTrigger("Death");
        }
    }
    public void Heal(int heal)
    {
        health.value += heal * healPercent;
    }
    public void Power(int _power)
    {
        power += _power;
    }
    public void Sheild(int _shield)
    {
        shield.value += _shield;
    }
}
