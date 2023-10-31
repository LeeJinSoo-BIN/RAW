using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public NPCspec spec;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setUP()
    {

    }

    void equipItem(GameObject player)
    {
        CharacterSpec spec = player.transform.GetComponent<MultyPlayer>().characterState.characterSpec;
        List<InventoryItem> equipment = spec.equipment;
        SPUM_SpriteList spriteList = player.GetComponentInChildren<SPUM_SpriteList>();
        foreach (InventoryItem item in equipment)
        {
            string current_item_sprite = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;
            spriteList.PartsPath[DataBase.Instance.itemInfoDict[item.itemName].itemType] = current_item_sprite;
            //Debug.Log(spriteList.PartsPath[itemInfoDict[item.itemName].itemType]);
        }
        spriteList._hairAndEyeColor = spec.colors;
        spriteList.setSprite();

    }
}
