using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MonsterSpec : ScriptableObject
{
    public string monsterName;
    public string monsterType;
    public string monsterDirectory;

    public float maxHealth;
    public float patternCycle;
    public List<MonsterSkillSpec> skillList;

    public string defaultMovement;
    public Vector3 defaultScale;
    [System.Serializable]
    public class SerializeDictDropItems : CustomDict.SerializableDictionary<string, float>
    {

    }
    public SerializeDictDropItems dropItems;//item name, probablity;
}
