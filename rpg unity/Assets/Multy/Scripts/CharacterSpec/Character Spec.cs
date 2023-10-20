using CustomDict;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class CharacterSpec : ScriptableObject
{
    public string nickName;
    public string roll;
    public int characterLevel;
    public string lastTown;
    
    public float maxHealth;
    public float maxMana;
    public float recoverManaPerThreeSec;
    public float power;
    public float criticalDamage;
    public float criticalPercent;
    public float healPercent;
    [Serializable]
    public class SerializeDictSkillLevel : SerializableDictionary<string, int> { }
    public SerializeDictSkillLevel skillLevel;
    public int maxInventoryNum;    
    public List<InventoryItem> inventory;
    public List<InventoryItem> equipment;
    public List<Color> colors;
        
}