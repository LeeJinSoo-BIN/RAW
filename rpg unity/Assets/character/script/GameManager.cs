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

    //Entitys �ڵ� �з��� ����, ��ƼƼ �������� ��ųʸ��� ���� 
    public SerializeDictItem itemInfoDict;
    public SerializeDictSkill skillInfoDict;
    
    void Awake()
    {
        Instance = this;
        Screen.SetResolution(960, 540, false);
    }
}
