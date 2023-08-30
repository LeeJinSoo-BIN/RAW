using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkillSpec : ScriptableObject
{
    public GameObject skillView;
    public string skillName;
    public string castType;
    // 0: circle   1: bar   2: targeting    3: buff
    // 4: charging


    public Vector2 radius;
    public Vector2 range;

    public string animType; //attack1,2,3, skill1,2,3        
    public float delay;
    public float duration;
    public float coolDown;

    public float flatDeal;
    public float dealIncreasePerSkillLevel;
    public float dealIncreasePerPower;

    public float flatHeal;
    public float healIncreasePerSkillLevel;
    public float healIncreasePerPower;

    public float flatShield;
    public float shieldIncreasePerSkillLevel;
    public float shieldIncreasePerPower;

    public float flatPower;
    public float powerIncreasePerSkillLevel;
    public float powerIncreasePerPower;

    /*public skill_spec(int _castType, (float x, float y) _radius, (float x, float y) _range, int _animType, int _effectType)
        {
            castType = _castType;
            radius = _radius;
            range = _range;
            animType = _animType;
            effectType = _effectType;
    }*/
}
