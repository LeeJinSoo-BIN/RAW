using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.ComponentModel.Design.Serialization;
using TMPro;

public class MonsterState : MonoBehaviourPunCallbacks
{
    private MonsterSpec monsterSpec;    
    public Slider health;
    public Slider shield;
    private bool isDeath = false;
    private Transform damagePop;
    private Dictionary<string, int> skillLevel = new Dictionary<string, int>();

    void Awake()
    {
        monsterSpec = transform.GetComponent<MonsterControl>().monsterSpec;
        health = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Slider>();
        shield = transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<Slider>();
        float playerNum = GameObject.Find("Player Group").transform.childCount;
        float weight = 1f;
        if (monsterSpec.monsterType == "Boss")
            weight = playerNum;
        else if (monsterSpec.monsterType == "Normal")
            weight += (playerNum - 1) * 0.25f;
        health.maxValue = monsterSpec.maxHealth * weight;
        health.value = health.maxValue;
        shield.maxValue = health.maxValue;
        shield.value = 0;
        damagePop = transform.Find("damage");
    }

    [PunRPC]
    void MonsterDamage(int type, float value, float duration, bool isCritical)
    {
        if (type == 5 && monsterSpec.monsterType.ToLower() == "normal") // 기본공격
            value *= 5;
        else if (type == 0 && monsterSpec.monsterType.ToLower() == "normal") // 공격
            value *= 2;
        float _shield = shield.value;
        shield.value -= value;
        value -= _shield;
        Debug.Log(value);
        PopDamage(type, value, isCritical);
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

    void PopDamage(int type, float value, bool isCritical)
    {        
        GameObject damage = Instantiate(Resources.Load<GameObject>("Character/skills/damage"));
        damage.transform.position = damagePop.position;
        TMP_Text damageText = damage.GetComponentInChildren<TMP_Text>();
        damageText.text = value.ToString();
        if (type == 0 || type == 4 || type == 5)
        {
            damageText.color = new Color(1f, (100f / 255f), (100f / 255f));
            if (isCritical)
            {
                damageText.color = new Color(1f, (50f / 255f), (50f / 255f));
                damageText.fontStyle = FontStyles.Bold;
                damageText.fontSize = 0.4f;
            }
        }
        else if (type == 1)
        {
            damageText.color = new Color((100f / 255f), 1f, (100f / 255f));
        }
        else if (type == 2)
        {
            damageText.color = new Color((200f / 255f), (200f / 255f), (200f / 255f));
        }
        else if (type == 3)
        {
            damageText.color = new Color((128f / 255f), (128f / 255f), 1f);
        }        
    }
}
