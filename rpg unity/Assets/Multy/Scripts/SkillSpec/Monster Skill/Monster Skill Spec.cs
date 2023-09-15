using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MonsterSkillSpec : ScriptableObject
{
    public string skillName;
    public string skillDirectory;
    public string type;    
    public float[] flatDeal;
    public float[] increaseDealPerLevel;

    public float firstDelay;
    public float skillDuration;
    public int areaChildNum;

    public float speed;
    public bool run;
    public float distance;

    public string castPos;
    public float targetSideDistance;

    public string spawnPos;
    //public string animationName;
}
