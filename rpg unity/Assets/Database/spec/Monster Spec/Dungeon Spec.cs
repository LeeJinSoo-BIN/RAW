using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DungeonSpec : ScriptableObject
{    
    [Serializable]
    public struct monsterInfo
    {
        public string monsterName;
        public Vector2 monsterPos;
    }
    [Serializable]
    public class monsterList2D
    {        
        public List<monsterInfo> monsterList = new List<monsterInfo>();
    }
    [SerializeField]
    public List<monsterList2D> monsterInfoList = new List<monsterList2D>();
    public List<float> timeLimit;
}
