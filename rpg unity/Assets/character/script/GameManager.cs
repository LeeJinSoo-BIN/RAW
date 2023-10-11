using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static DataBase;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class SerializeDictRollNameToSpec : CustomDict.SerializableDictionary<string, CharacterSpec> { }

    public static GameManager Instance;
    public Transform inGameUI;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Text windowText;
    public GameObject PanelList;
    public string skillThumbnailPath = "Character/skills/thumbnails";
    public SerializeDictRollNameToSpec characterSpecDict;
    void Awake()
    {        
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
        PanelList.GetComponent<UIManager>().myCharacter = player;
        PanelList.GetComponent<UIManager>().SetUP();
        Debug.Log("set up ingame ui");
    }
    CharacterSpec loadCharacterSpec(string roll)
    {
        CharacterSpec spec = new CharacterSpec();
        CharacterSpec defaultSpec = characterSpecDict[roll];
        spec.nickName = defaultSpec.nickName;
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
        spec.colors = defaultSpec.colors;
        return spec;
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


    public void setResolution()
    {
        string selected_resolution_string = resolutionDropdown.options[resolutionDropdown.value].text;        
        string[] selected_resolution = selected_resolution_string.Split(" x ");
        bool window = false;
        if (windowText.text == "창모드")
            window = true;
        else
            window = false;
        Screen.SetResolution(int.Parse(selected_resolution[0]), int.Parse(selected_resolution[1]), window);
    }

    public void setWindow()
    {
        if (windowText.text == "창모드")
        {
            Screen.fullScreen = false;
            windowText.text = "전체화면";
        }
        else
        {
            Screen.fullScreen= true;
            windowText.text = "창모드";
        }
    }

    public void ClickQuitButton()
    {
        if (Application.isPlaying) 
            Application.Quit();
    }   
}
