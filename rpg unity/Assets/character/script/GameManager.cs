using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;    
    [System.Serializable]
    public class SerializeDictItem : CustomDict.SerializableDictionary<string, ItemSpec>
    {

    }
    [System.Serializable]
    public class SerializeDictSkill : CustomDict.SerializableDictionary<string, SkillSpec>
    {

    }
    [System.Serializable]
    public class SerializeDictRollNameToSpec : CustomDict.SerializableDictionary<string, CharacterSpec>
    {

    }

    //Entitys 코드 분류에 따라, 엔티티 프리팹을 딕셔너리에 저장 
    public SerializeDictItem itemInfoDict;
    public SerializeDictSkill skillInfoDict;
    public SerializeDictRollNameToSpec characterSpecDict;
    public Transform inGameUI;
    
    void Awake()
    {
        Instance = this;
        Screen.SetResolution(960, 540, false);
    }

    public void setup(GameObject player, string roll)
    {
        CharacterSpec loadedSpec = loadCharacterSpec(roll);
        player.GetComponent<MultyPlayer>().characterState.characterSpec = loadedSpec;
        player.GetComponent<MultyPlayer>().loadData();
        equipItem(player);
        Debug.Log("loaded player data");
        player.GetComponent<MultyPlayer>().characterState.setUp();
        Debug.Log("set up state");
        inGameUI.GetComponent<InGameUI>().myCharacter = player;
        inGameUI.GetComponent<InGameUI>().setUp();
        Debug.Log("set up ingame ui");
    }
    CharacterSpec loadCharacterSpec(string roll)
    {
        CharacterSpec spec = new CharacterSpec();
        CharacterSpec defaultSpec = characterSpecDict[roll];
        spec.maxHealth = defaultSpec.maxHealth;
        spec.maxMana = defaultSpec.maxMana;
        spec.recoverManaPerThreeSec = defaultSpec.recoverManaPerThreeSec;
        spec.power = defaultSpec.power;
        spec.criticalDamage = defaultSpec.criticalDamage;
        spec.criticalPercent = defaultSpec.criticalPercent;
        spec.healPercent = defaultSpec.healPercent;
        spec.maxInventoryNum = defaultSpec.maxInventoryNum;
        spec.characterLevel = defaultSpec.characterLevel;
        spec.skillLevel = defaultSpec.skillLevel;
        spec.inventory = defaultSpec.inventory;
        spec.equipment = defaultSpec.equipment;        

        return spec;
    }

    void equipItem(GameObject player)
    {
        CharacterSpec spec =  player.transform.GetComponent<MultyPlayer>().characterState.characterSpec;
        List<InventoryItem> equipment = spec.equipment;
        SPUM_SpriteList spriteList = player.GetComponentInChildren<SPUM_SpriteList>();

        foreach (InventoryItem item in equipment)
        {
            string current_item_sprite = itemInfoDict[item.itemName].spriteDirectory;
            spriteList.PartsPath[itemInfoDict[item.itemName].itemType] = current_item_sprite;
            Debug.Log(spriteList.PartsPath[itemInfoDict[item.itemName].itemType]);
            /*if (itemInfoDict[item.itemName].itemType == "hair")
            {
                hair_path[0] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "helmet1")
            {
                hair_path[1] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "helmet2")
            {
                hair_path[2] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "face hair")
            {
                hair_path[3] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "cloth")
            {
                cloth_path[0] = current_item_sprite;
                cloth_path[1] = current_item_sprite;
                cloth_path[2] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "armor")
            {
                armor_path[0] = current_item_sprite;
                armor_path[1] = current_item_sprite;
                armor_path[2] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "pant")
            {
                pant_path[0] = current_item_sprite;
                pant_path[1] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "weapon left")
            {
                weapon_path[0] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "weapon right")
            {
                weapon_path[1] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "shield left")
            {
                weapon_path[2] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "shield right")
            {
                weapon_path[3] = current_item_sprite;
            }
            else if (itemInfoDict[item.itemName].itemType == "back")
            {
                back_path[0] = current_item_sprite;
            }*/
        }
        
        /*spriteList._hairListString = hair_path;
        spriteList._clothListString = cloth_path;
        spriteList._armorListString = armor_path;
        spriteList._pantListString = pant_path;
        spriteList._weaponListString = weapon_path;
        spriteList._backListString = back_path;*/
        spriteList.ResyncData();
    }
}
