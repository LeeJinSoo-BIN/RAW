using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class ItemSpec : ScriptableObject
{    
    public string itemName;
    public string itemDescription;
    public string itemType;
    public string spriteDirectory;
    public int sellPrice;
    public int buyPrice;
    public int maxCarryAmount;

    public float recoveryHealth;
    public float recoveryMana;
    public float coolDown;

    public float increasePower;
    public float increaseMaxHealth;
    public float increaseMaxMana;
    public float increaseCriticalPercent;
    public float increaseCriticalDamage;
    public float increasePerReinforce;
}
