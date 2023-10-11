
using CustomDict;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AccountInfo : ScriptableObject
{
    public string accountId;    
    public List<CharacterSpec> characterList;
}
