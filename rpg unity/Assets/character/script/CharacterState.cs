using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using JetBrains.Annotations;

public class CharacterState : MonoBehaviourPunCallbacks, IPunObservable
{
    public int maxHealth = 1000;
    public int maxMana = 1000;
    public float power = 10f;
    public Slider health;
    public Slider shield;
    public float criticalPercent = 50f;
    public float criticalDamage = 1.2f;
    public Animator characterAnimator;
    public float healPercent = 1f;
    private bool isDeath = false;
    private bool isPlayer = false;
    public MultyPlayer playerControl;

    private Dictionary<string, int> skillLevel = new Dictionary<string, int>();

    void Awake()
    {
        health = transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        shield = transform.GetChild(0).GetChild(2).GetComponent<Slider>();
        characterAnimator = transform.parent.GetComponent<Animator>();
        health.maxValue = maxHealth;
        health.value = maxHealth;
        shield.maxValue = maxHealth;
        shield.value = 0;
        if (transform.parent.CompareTag("Player"))
            isPlayer = true;
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
                if (isPlayer)
                {
                    characterAnimator.SetBool("IsDeath", true);
                    playerControl.movable = false;
                    playerControl.attackable = false;
                }
                else
                {
                    transform.GetComponent<skillIpattern>().Death();
                }
            }
            if(type == 4 && !isPlayer)
            {
                transform.GetComponent<skillIpattern>().Bind();
            }
        }
        else if(type == 1 && !isDeath) //heal
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
