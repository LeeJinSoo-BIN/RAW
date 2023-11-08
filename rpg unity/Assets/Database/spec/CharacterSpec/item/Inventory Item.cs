using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public int count;
    public int reinforce;
}

public class QuickInventory
{
    public int kindCount = 0;
    public SortedSet<int> position = new SortedSet<int>();
}