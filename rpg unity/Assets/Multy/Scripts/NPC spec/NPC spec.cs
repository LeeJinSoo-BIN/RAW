using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCspec : ScriptableObject
{
    public string NpcName;
    public List<string> talk = new List<string>();
    public List<InventoryItem> equipments = new List<InventoryItem>();

    public List<InventoryItem> sellingItems = new List<InventoryItem>();
    public List<string> questList = new List<string>();


}
