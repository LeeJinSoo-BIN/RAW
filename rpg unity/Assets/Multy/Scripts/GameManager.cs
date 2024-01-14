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
    public bool isClearingMonster = false;

    private bool dead = false;
    private bool loadPrevState = false;
    private float prevHealth;
    private float prevMana;
    private float prevShield;

    GameObject monsterGroup;
    GameObject playerGroup;
    GameObject itemGroup;
    void Awake()
    {
        if (offLine)
        {
            PhotonNetwork.OfflineMode = offLine;
            PhotonNetwork.CreateRoom("offline");
            //Screen.SetResolution(960, 540, false);
        }
        playerGroup = GameObject.Find("Player Group");
        monsterGroup = GameObject.Find("Enemy Group");
        itemGroup = GameObject.Find("Item Field");
    }
    private void Start()
    {
        if (!dead)
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
        else
        {
            DataBase.Instance.myCharacter = null;
            DataBase.Instance.myCharacterControl = null;
            DataBase.Instance.myCharacterState = null;
        }
        newNetworkManager.Instance.gameManager = this;
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
        if (loadPrevState)
        {
            DataBase.Instance.myCharacterState.health.value = prevHealth;
            DataBase.Instance.myCharacterState.shield.value = prevShield;
            DataBase.Instance.myCharacterState.mana.value = prevMana;
        }
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            portalObject.ActivatePortal(false);
            if (DataBase.Instance.currentStage > 1)
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
        while (monsterGroup.transform.childCount > 0)
            yield return null;
        isClearingMonster = false;
        if (!PhotonNetwork.IsMasterClient)
            yield break;
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

    public void StageClear(bool killBoss)
    {
        portalObject.ActivatePortal(true);
        if (killBoss)
            UIManager.Instance.EndGame("clear");
    }
    public void ReGame(bool nextStage)
    {
        if (nextStage)
        {
            if (DataBase.Instance.myCharacterState.isDeath || DataBase.Instance.myCharacter == null)
                dead = true;
            else
            {
                prevHealth = DataBase.Instance.myCharacterState.health.value;
                prevShield = DataBase.Instance.myCharacterState.shield.value;
                prevMana = DataBase.Instance.myCharacterState.mana.value;
                loadPrevState = true;
            }
        }
        else
        {
            dead = false;
            loadPrevState = false;
        }
        foreach (Transform player in playerGroup.transform)
        {
            Destroy(player.gameObject);
        }
        foreach (Transform monster in monsterGroup.transform)
        {
            Destroy(monster.gameObject);
        }
        foreach (Transform item in itemGroup.transform)
        {
            Destroy(item.gameObject);
        }
        Start();
        if (DataBase.Instance.currentStage == 1)
            SpawnMonster();
    }
}