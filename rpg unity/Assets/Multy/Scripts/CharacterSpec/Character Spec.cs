using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[System.Serializable]
public class skillLevelDict
{
    public string skillName;
    public int level;
}
*/

public class ScriptableAsset<TKey, TValue> : ScriptableObject
{
    public List<TKey> _listKey = new List<TKey>();
    public List<TValue> _listValue = new List<TValue>();
}

// key에 string, value에 objectInfo가 들어가는 asset
public class ObjectInfoAsset : ScriptableAsset<string, int> { }


[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    public List<TKey> keys = new List<TKey>();
    [SerializeField]
    public List<TValue> values = new List<TValue>();

    // serialize하기전에 dictionary에 있는 key값과 value를 옮겨 넣는다.
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        var enumer = GetEnumerator();
        while (enumer.MoveNext())
        {
            keys.Add(enumer.Current.Key);
            values.Add(enumer.Current.Value);
        }
    }
    // Deserialize하고 나서 List에 있는 key값과 value를 dictionary에 옮겨 넣는다.
    public void OnAfterDeserialize()
    {
        this.Clear();
        int keysCount = keys.Count;
        int valuesCount = values.Count;

        if (keysCount == 0)
        {
            return;
        }

        for (int i = 0; i < keysCount; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }

}
//public class DataInfoAsset : ScriptableAsset<int, DictionaryOfStringAndString> { }


/*[Serializable]
public class SerializeDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
{
    [SerializeField]
    List<K> keys = new List<K>();

    [SerializeField]
    List<V> values = new List<V>();
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        var enumer = GetEnumerator();
        while (enumer.MoveNext())
        {
            keys.Add(enumer.Current.Key);
            values.Add(enumer.Current.Value);
        }
    }
    public void OnAfterDeserialize()
    {
        this.Clear();
        if(keys.Count != values.Count)
            throw new Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
        for (int i = 0, icount = keys.Count; i < icount; ++i)
        {
            this.Add(keys[i], values[i]);
        }
    }
}
*/
[Serializable]
//public class SerializeDicString : SerializeDictionary<string, int> { }


[CreateAssetMenu]
public class CharacterSpec : ScriptableObject
{
    public float maxHealth = 1000f;
    public float power = 1f;
    public float criticalDamage = 1.2f;
    public float criticalPercent = 50f;
    public float healPercent = 1f;
    //public SerializeDicString skillLevel = new SerializeDicString();
    public Dictionary<string, int> skillLevel;
    public int maxInventoryNum = 24;
    //public List<skillLevelDict> skillLevel = new List<skillLevelDict>();
    
}
