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
    public int exp;
    public string lastTown;
    public int maxInventoryNum;

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
    
    public List<InventoryItem> inventory = new List<InventoryItem>(24);
    
    public List<InventoryItem> equipment = new List<InventoryItem>();
    public List<Color> colors = new List<Color> { Color.black, Color.black, Color.black, Color.black };

    public int money;

}