using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkillSpec : ScriptableObject
{
    public GameObject skillView;
    public string skillName;
    public string castType;

    public Vector2 radius;
    public Vector2 range;

    public string animType; //attack1,2,3, skill1,2,3        
    public float skillDelay;
    public float skillDuration;
    public float coolDown;

    public int skillLevel;
}
