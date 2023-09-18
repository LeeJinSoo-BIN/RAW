using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public int count;
    public int position;
    public int reinforce;
}

public class qucikInventoryInfo
{
    public int count;
    public int position;
}
