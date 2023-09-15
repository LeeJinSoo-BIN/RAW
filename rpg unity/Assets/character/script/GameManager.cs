using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CharacterSpec defaultSpec;
    [System.Serializable]
    public class SerializeDictItem : CustomDict.SerializableDictionary<string, ItemSpec>
    {

    }
    [System.Serializable]
    public class SerializeDictSkill : CustomDict.SerializableDictionary<string, SkillSpec>
    {

    }

    //Entitys 코드 분류에 따라, 엔티티 프리팹을 딕셔너리에 저장 
    public SerializeDictItem itemInfoDict;
    public SerializeDictSkill skillInfoDict;

    void Awake()
    {
        Instance = this;
        Screen.SetResolution(960, 540, false);
    }

    public void setUpCharacter(GameObject player)
    {
        CharacterSpec loadedSpec = loadCharacterSpec();
        player.GetComponent<MultyPlayer>().characterState.characterSpec = loadedSpec;
        player.GetComponentInChildren<CharacterState>().setUp();
        player.GetComponent<MultyPlayer>().loadData();
    }
    CharacterSpec loadCharacterSpec()
    {
        CharacterSpec spec = new CharacterSpec();
        spec.maxHealth = defaultSpec.maxHealth;
        spec.maxMana = defaultSpec.maxMana;
        spec.recoverManaPerThreeSec = defaultSpec.recoverManaPerThreeSec;
        spec.power = defaultSpec.power;
        spec.criticalDamage = defaultSpec.criticalDamage;
        spec.criticalPercent = defaultSpec.criticalPercent;
        spec.healPercent = defaultSpec.healPercent;
        spec.maxInventoryNum = defaultSpec.maxInventoryNum;
        spec.characterLevel = defaultSpec.characterLevel;        
        

        

        return spec;
    }
}
