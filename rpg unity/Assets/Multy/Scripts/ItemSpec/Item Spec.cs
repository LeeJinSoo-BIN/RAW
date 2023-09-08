using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PotionSpec : ScriptableObject
{    
    public string itemName;
    public string itemDescription;
    public int maxCarryAmount;
    public float recoveryHealthAmount;
    public float recoveryManaAmount;
    public float coolDown;
}
