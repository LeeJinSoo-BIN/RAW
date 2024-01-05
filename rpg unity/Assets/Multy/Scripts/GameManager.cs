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
        newNetworkManager.Instance.gameManager = this;
        
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
        //newNetworkManager.Instance.PV.RPC("UpdateParty", RpcTarget.All);
        //Debug.Log("set up ingame ui");
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            portalObject.ActivatePortal(false);
            DataBase.Instance.isInDungeon = true;
            if (DataBase.Instance.currentStage > 1 && PhotonNetwork.IsMasterClient)
                SpawnMonster();
        }
    }

    public void SpawnMonster()
    {
        StartCoroutine(spawnDelay());
    }
    IEnumerator spawnDelay()
    {
        float _timer = 0f;
        while (_timer < 0.2)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        foreach (DungeonSpec.monsterInfo monster in DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].monsterInfoList[DataBase.Instance.currentStage - 1].monsterList)
        {
            string monsterName = monster.monsterName;
            Vector2 montserPos = monster.monsterPos;
            GameObject spawnedMonster = PhotonNetwork.InstantiateRoomObject("Monster/" + monsterName, montserPos, Quaternion.identity);
            if (spawnedMonster.GetComponent<MonsterControl>().monsterSpec.monsterType.ToLower() == "boss")
                newNetworkManager.Instance.PV.RPC("SpawnBoss", RpcTarget.All);
        }
        if (DataBase.Instance.currentStage == 1)
        {
            newNetworkManager.Instance.PV.RPC("startTimer", RpcTarget.AllBuffered);
        }
    }

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
}
