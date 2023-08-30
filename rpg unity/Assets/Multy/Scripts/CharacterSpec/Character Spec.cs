using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterSpec : ScriptableObject
{
    public float maxHealth = 1000f;
    public float power = 1f;
    public float criticalDamage = 1.2f;
    public float criticalPercent = 50f;
    public float healPercent = 1f;
    public Dictionary<string, int> skillLevel;
    
}
