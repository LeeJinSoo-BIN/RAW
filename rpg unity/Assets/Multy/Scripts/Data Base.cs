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

    public bool isLogined = false;
    public bool isInDungeon = false;

    public AccountInfo defaultAccountInfo;
    public AccountInfo accountInfo;
    public CharacterSpec selectedCharacterSpec;
    public string currentMapName;    
    public string currentMapType;
    public int currentStage;

    public GameObject myCharacter;
    public MultyPlayer myCharacterControl;
    public CharacterState myCharacterState;

    public string myPartyCaptainName;
    public string myPartyName;
    public bool isCaptain;
    public int myPartyMemNum;

    public bool usingCheat = false;
    public bool isPromotioned = false;
    private void Awake()
    {
        var obj = FindObjectsOfType<DataBase>();                                                                                                                                                        
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            defaultAccountInfo = ScriptableObject.CreateInstance<AccountInfo>();
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
    public int CalEnchantPercent(int currentReinforce)
    {
        if (currentReinforce <= 4)
            return 90 - 5 * currentReinforce;
        else if (currentReinforce <= 7)
            return 100 - 10 * currentReinforce;
        else if (currentReinforce <= 9)
            return 30;
        else if (currentReinforce <= 11)
            return (12 - currentReinforce) * 10;
        else
            return 0;

    }
    public int[] CalEnchantPrice(string itemName, int currentReinforce)
    {
        int[] price = new int[2];
        price[0] = 100 * (currentReinforce + 1);
        price[1] = 100 * (currentReinforce + 1);
        return price;
    }

    public int CalEnchantPower(string itemName, int reinforce)
    {
        if (reinforce < 5)
        {
            return (int)itemInfoDict[itemName].increasePerReinforce;
        }
        else if (5 <= reinforce && reinforce <= 7)
        {
            return (int)itemInfoDict[itemName].increasePerReinforce + reinforce / 2;
        }
        else
            return (int)itemInfoDict[itemName].increasePerReinforce + reinforce;
    }
}
