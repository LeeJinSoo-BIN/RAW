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


    private void Awake()
    {
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
