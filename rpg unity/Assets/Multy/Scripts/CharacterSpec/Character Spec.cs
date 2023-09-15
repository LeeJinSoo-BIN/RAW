using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class CharacterSpec : ScriptableObject
{
    public float maxHealth = 1000f;
    public float maxMana = 1000f;
    public float recoverManaPerThreeSec = 5f;
    public float power = 1f;
    public float criticalDamage = 1.2f;
    public float criticalPercent = 50f;
    public float healPercent = 1f;
    [System.Serializable]
    public class SerializeDictSkillLevel : CustomDict.SerializableDictionary<string, int>
    {

    }
    public SerializeDictSkillLevel skillLevel;
    public int maxInventoryNum = 24;
    public int characterLevel = 1;    
}