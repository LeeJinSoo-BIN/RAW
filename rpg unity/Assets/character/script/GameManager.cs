using System.Collections;
using System.Collections.Generic;
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

    //Entitys 코드 분류에 따라, 엔티티 프리팹을 딕셔너리에 저장 
    public SerializeDictItem itemInfoDict;
    public SerializeDictSkill skillInfoDict;
    
    void Awake()
    {
        Instance = this;
        Screen.SetResolution(960, 540, false);
    }
}
