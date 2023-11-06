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
    public int exp;
    [Serializable]
    public class SerializeDictSkillLevel : SerializableDictionary<string, int> { }
    public SerializeDictSkillLevel skillLevel;
    public int maxInventoryNum;    
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public List<InventoryItem> equipment = new List<InventoryItem>();
    public List<Color> colors = new List<Color> { Color.black, Color.black, Color.black };

    public int money;

}