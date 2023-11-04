using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NpcSpec : ScriptableObject
{
    public string NpcName;
    public string NpcType;    
    public List<InventoryItem> equipments = new List<InventoryItem>();
    public List<Color> colors = new List<Color>();

    public List<string> ments = new List<string>();
    public List<InventoryItem> sellingItems = new List<InventoryItem>();
    public List<string> questList = new List<string>();


}
