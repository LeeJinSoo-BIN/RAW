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
    public Slider mana;
    public Slider shield;
    public float power;
    public Animator characterAnimator;
    
    private bool isDeath = false;
    public MultyPlayer playerControl;
    private float _timer = 0f;
    public PhotonView PV;
    public void setUp()
    {
        shield.maxValue = characterSpec.maxHealth;
        shield.value = 0;
        health.maxValue = characterSpec.maxHealth;
        health.value = characterSpec.maxHealth;
        mana.maxValue = characterSpec.maxMana;
        mana.value = characterSpec.maxMana;
        power = characterSpec.power;
    }

    // Update is called once per frame
    void Update()
    {
        if (PV.IsMine)
        {
            _timer += Time.deltaTime;
            if (_timer >= 3f)
            {
                _timer = 0f;
                mana.value += characterSpec.recoverManaPerThreeSec;
            }
        }
    }
    
    public void ProcessSkill(int type, float value)
    {
        if (!isDeath)
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
                    playerControl.isDeath = true;
                }
                if (type == 4)
                {
                    //transform.GetComponent<EvilWizard>().Bind();
                }
            }
            else if (type == 1) //heal
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
            else if (type == 5) // mana
            {
                mana.value += value;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health.value);
            stream.SendNext(shield.value);
            stream.SendNext(mana.value);
        }
        else
        {
            health.value = (float)stream.ReceiveNext();
            shield.value = (float)stream.ReceiveNext();
            mana.value = (float)stream.ReceiveNext();
        }
    }
}
