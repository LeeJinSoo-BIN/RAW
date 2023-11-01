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
    

    public SerializeDictItem itemInfoDict;
    public SerializeDictSkill skillInfoDict;
    public string skillThumbnailPath = "Character/skills/thumbnails";

    public bool isLogined = false;

    public AccountInfo defaultAccountInfo;
    public AccountInfo accountInfo;
    public CharacterSpec selectedCharacterSpec;
    public string currentMapName;
    public string currentMapType;
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
        defaultAccountInfo.characterList.Clear();
        var obj = FindObjectsOfType<DataBase>();
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if(Instance == null)
            Instance = this;
    }
}
