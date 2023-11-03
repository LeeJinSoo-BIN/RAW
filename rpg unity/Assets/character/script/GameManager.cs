using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using WebSocketSharp;

public class GameManager : MonoBehaviour
{   
    public newNetworkManager networkManager;
    public bool offLine = false;
    void Awake()
    {
        if (offLine)
        {
            PhotonNetwork.OfflineMode = offLine;
            PhotonNetwork.CreateRoom("offline");
            //Screen.SetResolution(960, 540, false);
        }
    }
        private void Start()
    {
        GameObject player = PhotonNetwork.Instantiate("Character/Player", Vector3.zero, Quaternion.identity);
        DataBase.Instance.myCharacter = player;
        DataBase.Instance.myCharacterControl = player.GetComponent<MultyPlayer>();
        DataBase.Instance.myCharacterState = player.GetComponent<CharacterState>();

        if (GameObject.Find("EasterEgg") != null)
        {
            GameObject.Find("EasterEgg").GetComponent<EasterEgg>().myCharacter = player;
        }
        setup(player);
        GameObject.Find("Main Camera").transform.GetComponent<CameraFollow>().myCharacterTransform = player.transform;

    }

    public void setup(GameObject player)
    {
        CharacterSpec loadedSpec = DataBase.Instance.selectedCharacterSpec;
        player.GetComponent<MultyPlayer>().characterState.characterSpec = loadedSpec;
        player.GetComponent<MultyPlayer>().loadData();
        equipItem(player);
        Debug.Log("loaded player data");
        player.GetComponent<MultyPlayer>().characterState.setUp();
        Debug.Log("set up state");
        UIManager.Instance.SetUP();
        Debug.Log("set up ingame ui");
        if (DataBase.Instance.currentMapType == "dungeon" && PhotonNetwork.LocalPlayer.IsMasterClient)
            StartCoroutine(SpawnBoss());
    }

    void equipItem(GameObject player)
    {
        CharacterSpec spec = player.transform.GetComponent<MultyPlayer>().characterState.characterSpec;
        List<InventoryItem> equipment = spec.equipment;
        SPUM_SpriteList spriteList = player.GetComponentInChildren<SPUM_SpriteList>();
        spriteList.resetSprite();
        foreach (InventoryItem item in equipment)
        {
            string current_item_sprite = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;
            spriteList.PartsPath[DataBase.Instance.itemInfoDict[item.itemName].itemType] = current_item_sprite;
            //Debug.Log(spriteList.PartsPath[itemInfoDict[item.itemName].itemType]);
        }
        spriteList._hairAndEyeColor = spec.colors;        
        spriteList.setSprite();
    }


    public void ReGame()
    {
        foreach (Transform player in GameObject.Find("Player Group").transform)
        {
            Destroy(player.gameObject);
        }
        foreach (Transform monster in GameObject.Find("Enemy Group").transform)
        {
            Destroy(monster.gameObject);
        }
        foreach(Transform item in GameObject.Find("Item Field").transform)
        {
            Destroy(item.gameObject);
        }
        Start();
    }

    IEnumerator SpawnBoss()
    {
        float _timer = 0f;
        while (_timer < 5f)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        //PhotonNetwork.Instantiate("Monster/Evil Wizard", Vector3.zero, Quaternion.identity);
        PhotonNetwork.InstantiateRoomObject("Monster/Evil Wizard", Vector3.zero, Quaternion.identity);
        networkManager.PV.RPC("SpawnBoss", RpcTarget.All);
    }       
}
