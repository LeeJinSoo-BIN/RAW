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
    public portal portalObject;
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
        //Debug.Log("loaded player data");
        player.GetComponent<MultyPlayer>().characterState.setUp();
        //Debug.Log("set up state");
        UIManager.Instance.SetUP();
        //Debug.Log("set up ingame ui");
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            portalObject.ActivatePortal(false);
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                StartCoroutine(SpawnMonster());
                if(DataBase.Instance.currentStage == 1)
                {
                    networkManager.PV.RPC("startTimer", RpcTarget.AllBuffered);
                }
            }

        }
    }

    /*void equipItem(GameObject player)
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
    }*/

    public void StageClear()
    {
        portalObject.ActivatePortal(true);
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

    IEnumerator SpawnMonster()
    {
        float _timer = 0f;
        while (_timer < (DataBase.Instance.currentStage == 1 ? 3 : 0.1))
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        foreach (DungeonSpec.monsterInfo monster in DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].monsterInfoList[DataBase.Instance.currentStage - 1].monsterList)
        {
            string monsterName = monster.monsterName;
            Vector2 montserPos = monster.monsterPos;
            GameObject spawnedMonster =  PhotonNetwork.InstantiateRoomObject("Monster/" + monsterName, montserPos, Quaternion.identity);
            if(spawnedMonster.GetComponent<MonsterControl>().monsterSpec.monsterType.ToLower() == "boss")
                networkManager.PV.RPC("SpawnBoss", RpcTarget.All);
        }
    }
}
