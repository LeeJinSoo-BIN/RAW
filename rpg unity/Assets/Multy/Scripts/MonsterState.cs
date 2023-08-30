using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MonsterState : MonoBehaviourPunCallbacks
{
    public int maxHealth = 1000;
    public int maxMana = 1000;
    public float power = 10f;
    public Slider health;
    public Slider shield;
    public float criticalPercent = 50f;
    public float criticalDamage = 1.2f;
    public float healPercent = 1f;
    private bool isDeath = false;

    private Dictionary<string, int> skillLevel = new Dictionary<string, int>();

    void Awake()
    {
        health = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Slider>();
        shield = transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Slider>();
        health.maxValue = maxHealth;
        health.value = maxHealth;
        shield.maxValue = maxHealth;
        shield.value = 0;
    }

    [PunRPC]
    void MonsterDamage(int type, float value, float duration)
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
            transform.GetComponent<skillIpattern>().Death();
        }
        if(type == 4)
        {
            transform.GetComponent<skillIpattern>().Bind(duration);            
        }
        transform.GetComponent<skillIpattern>().Hit();
    }
}
