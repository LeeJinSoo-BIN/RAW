using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    public static DataBase Instance;
    [System.Serializable]
    public class SerializeDictItem : CustomDict.SerializableDictionary<string, ItemSpec> { }

    [System.Serializable]
    public class SerializeDictSkill : CustomDict.SerializableDictionary<string, SkillSpec> { }


    [System.Serializable]
    public class SerializeDictDungeonInfo : CustomDict.SerializableDictionary<string, DungeonSpec> { }

    public SerializeDictItem itemInfoDict;
    public SerializeDictSkill skillInfoDict;
    public SerializeDictDungeonInfo dungeonInfoDict;
    public string skillThumbnailPath = "Character/skills/thumbnails";

    public bool isLogined = false;

    public AccountInfo defaultAccountInfo;
    public AccountInfo accountInfo;
    public CharacterSpec selectedCharacterSpec;
    public string currentMapName;    
    public string currentMapType;
    public int currentStage;
    public bool isCurrentDungeonCaptain;

    public GameObject myCharacter;
    public MultyPlayer myCharacterControl;
    public CharacterState myCharacterState;

    public string myPartyCaptainName;
    public string myPartyName;
    public bool isCaptain;

    public bool usingCheat = false;
    public bool isPromotioned = false;
    private void Awake()
    {
        var obj = FindObjectsOfType<DataBase>();
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            defaultAccountInfo = new AccountInfo();
            defaultAccountInfo.accountId = "default";
            defaultAccountInfo.characterList = new List<CharacterSpec>();            
        }
        else
        {
            Destroy(gameObject);
        }
        if(Instance == null)
            Instance = this;
    }
}
