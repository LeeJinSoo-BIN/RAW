using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using JetBrains.Annotations;
using UnityEditor;

public class CharacterState : MonoBehaviourPunCallbacks, IPunObservable
{
    
    public CharacterSpec characterSpec;
    public Slider health;
    public Slider shield;
    public float power;
    public Animator characterAnimator;
    
    private bool isDeath = false;
    public MultyPlayer playerControl;
    private Dictionary<string, int> skillLevel = new Dictionary<string, int>();

    void Awake()
    {
        shield.maxValue = characterSpec.maxHealth;
        shield.value = 0;
        health.maxValue = characterSpec.maxHealth;
        health.value = characterSpec.maxHealth;
        
        skillLevel.Add("magic floor", 1);
        skillLevel.Add("magic totem", 1);
        skillLevel.Add("magic heal", 1);
        skillLevel.Add("magic global heal", 1);
        skillLevel.Add("arrow charge", 1);
        skillLevel.Add("arrow rain", 1);
        skillLevel.Add("arrow dash", 1);
        skillLevel.Add("arrow gatling", 1);
        skillLevel.Add("sword smash", 1);
        skillLevel.Add("sword shield", 1);
        skillLevel.Add("sword slash", 1);
        skillLevel.Add("sword bind", 1);
        characterSpec.skillLevel = skillLevel;
        power = characterSpec.power;
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
                playerControl.movable = false;
                playerControl.attackable = false;
            }
            if(type == 4)
            {
                transform.GetComponent<skillIpattern>().Bind();
            }
        }
        else if(type == 1 && !isDeath) //heal
        {
            health.value += value * characterSpec.healPercent;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health.value);
            stream.SendNext(shield.value);
        }
        else
        {
            health.value = (float)stream.ReceiveNext();
            shield.value = (float)stream.ReceiveNext();
        }
    }
}
