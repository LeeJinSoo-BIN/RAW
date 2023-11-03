using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NpcSpec spec;
    public TMP_Text npcName;
    void Start()
    {
        setup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setup()
    {
        if(spec.NpcType == "store")
        {
            npcName.text = "[상인]\n";            
        }
        else
        {
            npcName.text = "";
        }
        npcName.text += spec.NpcName;
        equipItem();
    }

    void equipItem()
    {
        List<InventoryItem> equipment = spec.equipments;
        SPUM_SpriteList spriteList = gameObject.GetComponentInChildren<SPUM_SpriteList>();
        spriteList.resetSprite();
        foreach (InventoryItem item in equipment)
        {
            string current_item_sprite = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;
            spriteList.PartsPath[DataBase.Instance.itemInfoDict[item.itemName].itemType] = current_item_sprite;
            //Debug.Log(spriteList.PartsPath[DataBase.Instance.itemInfoDict[item.itemName].itemType]);
        }
        spriteList._hairAndEyeColor = spec.colors;
        spriteList.setSprite();
    }



    public void ClickNPC()
    {
        if(spec.NpcType == "store")
        {
            //set up store
            UIManager.Instance.storePanel.SetActive(true);
        }
        else if(spec.NpcType == "normal")
        {
            //set up conversation
            UIManager.Instance.conversationPanel.SetActive(true);
        }        
    }
}
