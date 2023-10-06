using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using JetBrains.Annotations;
using UnityEditor;
using TMPro;

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
        PV.RPC("syncMax", RpcTarget.AllBuffered, "shield", characterSpec.maxHealth);
        PV.RPC("syncMax", RpcTarget.AllBuffered, "health", characterSpec.maxHealth);
        PV.RPC("syncMax", RpcTarget.AllBuffered, "mana", characterSpec.maxMana);        
        power = characterSpec.power;
    }
    [PunRPC]
    void syncMax(string what, float max)
    {
        if(what == "health")
        {
            health.maxValue = max;
            health.value = max;
        }
        else if(what == "shield")
        {
            shield.maxValue = max;
            shield.value = 0;
        }
        else if(what == "mana")
        {
            mana.maxValue = max;
            mana.value = max;
        }
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
            PopDamage(type, value);
            if (type == 0 || type == 4) // damage
            {
                float _shield = shield.value;
                shield.value -= value;
                value -= _shield;
                print(value);
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
    void PopDamage(int type, float value)
    {
        Debug.Log("pop");
        GameObject damage = Instantiate(Resources.Load<GameObject>("Character/skills/damage"));
        damage.transform.position = new Vector3(transform.position.x, transform.position.y + 1.3f);
        TMP_Text damageText = damage.GetComponentInChildren<TMP_Text>();
        damageText.text = ((int)value).ToString();
        if(type == 0 || type == 4)
        {
            damageText.color = new Color(1f, (100f / 255f), (100f / 255f));
        }
        else if(type == 1)
        {
            damageText.color = new Color((100f / 255f), 1f, (100f / 255f));
        }
        else if(type == 2)
        {
            damageText.color = new Color((200f / 255f), (200f / 255f), (200f / 255f));
        }
        else if( type == 3)
        {
            damageText.color = new Color((128f / 255f), (128f / 255f), 1f);
        }
        else if(type == 5)
        {
            damageText.color = new Color((10f / 255f), (100f / 255f), 1f);
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
