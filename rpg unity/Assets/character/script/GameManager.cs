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

    //Entitys �ڵ� �з��� ����, ��ƼƼ �������� ��ųʸ��� ���� 
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

        

        return spec;
    }
}
