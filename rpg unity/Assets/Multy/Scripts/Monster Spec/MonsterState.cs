using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MonsterState : MonoBehaviourPunCallbacks
{
    private MonsterSpec monsterSpec;    
    public Slider health;
    public Slider shield;
    private bool isDeath = false;

    private Dictionary<string, int> skillLevel = new Dictionary<string, int>();

    void Awake()
    {
        monsterSpec = transform.GetComponent<MonsterControl>().monsterSpec;
        health = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Slider>();
        shield = transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Slider>();
        health.maxValue = monsterSpec.maxHealth;
        health.value = monsterSpec.maxHealth;
        shield.maxValue = monsterSpec.maxHealth;
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
            transform.GetComponent<MonsterControl>().Death();
        }
        if (type == 4)        
            transform.GetComponent<MonsterControl>().Bind(duration);        
        transform.GetComponent<MonsterControl>().Hit();
    }
}