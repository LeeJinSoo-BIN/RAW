using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;
using CustomDict;
using System;
using Unity.VisualScripting;

public class SPUM_SpriteList : MonoBehaviour
{
    public List<SpriteRenderer> _eyeList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _hairList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _bodyList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _clothList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _armorList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _pantList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _weaponList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _backList = new List<SpriteRenderer>();
    // Start is called before the first frame update

    public List<string> _hairListString = new List<string>();
    public List<string> _clothListString = new List<string>();
    public List<string> _armorListString = new List<string>();
    public List<string> _pantListString = new List<string>();
    public List<string> _weaponListString = new List<string>();
    public List<string> _backListString = new List<string>();

    public List<Color> _hairAndEyeColor = new List<Color>();
    [Serializable]
    public class SerializeDictPartsToSpriteRenderer : SerializableDictionary<string, SpriteRenderer> { }
    public SerializeDictPartsToSpriteRenderer BodyParts;

    [Serializable]
    public class SerializeDictPartsToPath : SerializableDictionary<string, string> { }
    public SerializeDictPartsToPath PartsPath;


    public PhotonView PV;


    public void Reset()
    {
        for (var i = 0; i < _hairList.Count; i++)
        {
            if (_hairList[i] != null) _hairList[i].sprite = null;
        }
        for (var i = 0; i < _clothList.Count; i++)
        {
            if (_clothList[i] != null) _clothList[i].sprite = null;
        }
        for (var i = 0; i < _armorList.Count; i++)
        {
            if (_armorList[i] != null) _armorList[i].sprite = null;
        }
        for (var i = 0; i < _pantList.Count; i++)
        {
            if (_pantList[i] != null) _pantList[i].sprite = null;
        }
        for (var i = 0; i < _weaponList.Count; i++)
        {
            if (_weaponList[i] != null) _weaponList[i].sprite = null;
        }
        for (var i = 0; i < _backList.Count; i++)
        {
            if (_backList[i] != null) _backList[i].sprite = null;
        }
    }


    public void LoadSpriteStingProcess(List<SpriteRenderer> SpList, List<string> StringList)
    {
        for (var i = 0; i < StringList.Count; i++)
        {
            if (StringList[i].Length > 1)
            {

                // Assets/SPUM/SPUM_Sprites/BodySource/Species/0_Human/Human_1.png
            }
        }
    }

    public void LoadSprite(SPUM_SpriteList data)
    {
        //스프라이트 데이터 연동
        SetSpriteList(_hairList, data._hairList);
        SetSpriteList(_bodyList, data._bodyList);
        SetSpriteList(_clothList, data._clothList);
        SetSpriteList(_armorList, data._armorList);
        SetSpriteList(_pantList, data._pantList);
        SetSpriteList(_weaponList, data._weaponList);
        SetSpriteList(_backList, data._backList);

        //색 데이터 연동.
        _eyeList[0].color = data._eyeList[0].color;
        _eyeList[1].color = data._eyeList[1].color;
        _hairList[3].color = data._hairList[3].color;
        _hairList[0].color = data._hairList[0].color;
        //꺼져있는 오브젝트 데이터 연동.
        _eyeList[0].gameObject.SetActive(!data._eyeList[0].gameObject.activeInHierarchy);
        _eyeList[1].gameObject.SetActive(!data._eyeList[1].gameObject.activeInHierarchy);
        _hairList[0].gameObject.SetActive(!data._hairList[0].gameObject.activeInHierarchy);
        _hairList[3].gameObject.SetActive(!data._hairList[3].gameObject.activeInHierarchy);
    }

    public void SetSpriteList(List<SpriteRenderer> tList, List<SpriteRenderer> tData)
    {
        for (var i = 0; i < tData.Count; i++)
        {
            if (tData[i] != null) tList[i].sprite = tData[i].sprite;
            else tList[i] = null;
        }
    }
    public void setSprite()
    {
        foreach (string part in PartsPath.Keys)
        {
            if (part.Contains("weapon"))
            {
                if (PV != null)
                {
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, "weapon left", null);
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, "weapon right", null);                    
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, part, PartsPath[part]);
                }
                else
                {
                    changeSprite("weapon left", null);
                    changeSprite("weapon right", null);                    
                    changeSprite(part, PartsPath[part]);
                }
            }
            else
            {
                if (PV != null)
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, part, PartsPath[part]);
                else
                    changeSprite(part, PartsPath[part]);
            }
        }
        for (int k = 0; k < 4; k++)
        {
            if(PV != null)
                PV.RPC("setColors", RpcTarget.AllBuffered, k, _hairAndEyeColor[k].r, _hairAndEyeColor[k].g, _hairAndEyeColor[k].b);
            else
                setColors(k, _hairAndEyeColor[k].r, _hairAndEyeColor[k].g, _hairAndEyeColor[k].b);
        }
    }

    public void resetSprite()
    {
        PartsPath.Clear();
        foreach (string part in BodyParts.SD_Keys)
        {
            if (part.Contains("weapon"))
            {
                if (PV != null)
                {
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, "weapon left", null);
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, "weapon right", null);
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, part, null);
                }
                else
                {
                    changeSprite("weapon left", null);
                    changeSprite("weapon right", null);
                    changeSprite(part, null);
                }
            }
            else
            {
                if (PV != null)
                    PV.RPC("changeSprite", RpcTarget.AllBuffered, part, null);
                else
                    changeSprite(part, null);
            }
        }
    }


    public void ResyncData()
    {
        Debug.Log("Resync");
        SyncPath(_hairList, _hairListString);
        Debug.Log("hair");
        SyncPath(_clothList, _clothListString);
        Debug.Log("cloth");
        SyncPath(_armorList, _armorListString);
        Debug.Log("armor");
        SyncPath(_pantList, _pantListString);
        Debug.Log("pant");
        SyncPath(_weaponList, _weaponListString);
        Debug.Log("weapon");
        SyncPath(_backList, _backListString);
        Debug.Log("back");
    }
    public void SyncPath(List<SpriteRenderer> _objList, List<string> _pathList)
    {
        for (var i = 0; i < _pathList.Count; i++)
        {

            if (_pathList[i].Length > 1)
            {
                string tPath = _pathList[i];
                tPath = tPath.Replace("Assets/character/", "");
                tPath = tPath.Replace(".png", "");
                Debug.Log(tPath);
                Sprite[] tSP = Resources.LoadAll<Sprite>(tPath);
                Debug.Log(tSP.Length);
                if (tSP.Length > 1)
                {
                    _objList[i].sprite = tSP[i];
                }
                else
                {
                    _objList[i].sprite = tSP[0];
                }
            }
            else
            {
                _objList[i].sprite = null;
            }
        }
    }

    [PunRPC]
    public void changeSprite(string part, string path)
    {
        if (path == null) {
            if (part == "cloth")
            {
                BodyParts["cloth body"].sprite = null;
                BodyParts["cloth left arm"].sprite = null;
                BodyParts["cloth right arm"].sprite = null;
            }
            else if (part == "armor")
            {
                BodyParts["armor body"].sprite = null;
                BodyParts["armor left shoulder"].sprite = null;
                BodyParts["armor right shoulder"].sprite = null;
            }
            else if (part == "pant")
            {
                BodyParts["pant left"].sprite = null;
                BodyParts["pant right"].sprite = null;
            }
            else if (part == "shoulder")
            {
                BodyParts["shoulder left"].sprite = null;
                BodyParts["shoulder right"].sprite = null;
            }
            else if(part == "eye")
            {
                BodyParts["eye left"].sprite = null;
                BodyParts["eye right"].sprite = null;
            }
            else
            {
                BodyParts[part].sprite = null;
            }
            return;
        }
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        if (part == "cloth")
        {
            BodyParts["cloth body"].sprite = sprites[0];
            BodyParts["cloth left arm"].sprite = sprites[1];
            BodyParts["cloth right arm"].sprite = sprites[2];
        }
        else if (part == "armor")
        {
            BodyParts["armor body"].sprite = sprites[0];
            BodyParts["armor left shoulder"].sprite = sprites[1];
            BodyParts["armor right shoulder"].sprite = sprites[2];
        }
        else if (part == "pant")
        {
            BodyParts["pant left"].sprite = sprites[0];
            BodyParts["pant right"].sprite = sprites[1];
        }
        else if(part == "shoulder")
        {
            BodyParts["shoulder left"].sprite = sprites[0];
            BodyParts["shoulder right"].sprite = sprites[1];
        }
        else if (part == "eye")
        {
            BodyParts["eye left"].sprite = sprites[0];
            BodyParts["eye right"].sprite = sprites[1];
        }
        else
        {
            BodyParts[part].sprite = sprites[0];
        }
    }
    [PunRPC]
    public void setColors(int which, float r, float g, float b)
    {
        if (which == 0)
            _hairList[0].color = new Color(r, g, b);
        else if (which == 1)
            _eyeList[0].color = new Color(r, g, b);
        else if (which == 2)
            _eyeList[1].color = new Color(r, g, b);
        else if (which == 3)
            _hairList[3].color = new Color(r, g, b);
    }

}
