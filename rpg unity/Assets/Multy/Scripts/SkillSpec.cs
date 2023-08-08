using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkillSpec : ScriptableObject
{
    public GameObject skillView;
    public string castType;
    // 0: circle   1: bar   2: targeting    3: buff
    // 4: charging


    public Vector2 radius;
    public Vector2 range;

    public string animType; //attack1,2,3, skill1,2,3        
    public float skillDelay;
    public float skillDuration;
    /*public skill_spec(int _castType, (float x, float y) _radius, (float x, float y) _range, int _animType, int _effectType)
        {
            castType = _castType;
            radius = _radius;
            range = _range;
            animType = _animType;
            effectType = _effectType;
    }*/
}
