using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterState : MonoBehaviour
{
    private float maxHealth = 1000;
    private Slider health;
    private Animator characterAnimator;
    void Start()
    {
        health = transform.GetChild(1).GetComponent<Slider>();
        characterAnimator = transform.GetChild(0).GetComponent<Animator>();
        health.maxValue = maxHealth;
        health.value = maxHealth;

    }

    // Update is called once per frame
    public void changeHealth(float value)
    {
        health.value += value;
        if(health.value <= 0 )
        {            
            characterAnimator.SetBool("isDeath", true);
            characterAnimator.SetTrigger("Death");
        }
    }
}
