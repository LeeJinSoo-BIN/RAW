using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.EventSystems;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private byte maxPlayersPerRoom = 3;

    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespwanPanel;
    public GameObject ConnectPanel;

    public GameObject InGameUI;
    public GameObject ground;
    public GameObject itemField;
    private bool connecting = true;
    public TMP_Text ConnectButtonText;
    public PhotonView PV;
    public GameObject spawnButton;

    public GameObject roomInfo;
    public GameObject RoomList;
    public GameObject LobbyPanel;
    public TMP_InputField RoomNameInputField;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Connect();
        InGameUI.SetActive(false);
        ground.SetActive(false);
        itemField.SetActive(false);
        ConnectPanel.SetActive(false);
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();            
        }
        if (connecting)
        {
            ConnectButtonText.text = PhotonNetwork.NetworkClientState.ToString();
        }
    }
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        //connecting = true;
    }

    public void JoinLobby()
    {
        connecting = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnConnectedToMaster()
    {
        connecting = false;
        ConnectButtonText.text = "접속";
    }

    public override void OnJoinedLobby()
    {
        connecting = false;
        LobbyPanel.SetActive(true);
        DisconnectPanel.SetActive(false);
    }

    public override void OnCreatedRoom()
    {

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //print("room list updated");
        UpdateRoomList(roomList);
    }

    void UpdateRoomList(List<RoomInfo> roomList)
    {
        for (int k = 0; k < RoomList.transform.childCount; k++)
        {
            Destroy(RoomList.transform.GetChild(k).gameObject);
        }
        //print(roomList.Count);
        for (int k = 0; k < roomList.Count; k++)
        {
            if (roomList[k].RemovedFromList) continue;
            GameObject Room = Instantiate(roomInfo);

            Room.transform.SetParent(RoomList.transform);
            Room.transform.localScale = new Vector3(1, 1, 1);
            Room.SetActive(true);
            Room.transform.GetChild(0).GetComponent<TMP_Text>().text = roomList[k].Name;
            string count = roomList[k].PlayerCount + " / " + roomList[k].MaxPlayers;
            Room.transform.GetChild(1).GetComponent<TMP_Text>().text = count;
            if (roomList[k].PlayerCount == roomList[k].MaxPlayers || !roomList[k].IsOpen)
                Room.GetComponent<Button>().interactable = false;
        }
    }

    public void JoinRoomButtonClickInJoinPanel()
    {
        string room_name = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<TMP_Text>().text;
        PhotonNetwork.JoinRoom(room_name);
    }
    public override void OnLeftRoom()
    {
        connecting = false;
        LobbyPanel.SetActive(true);        

        InGameUI.SetActive(false);
        ground.SetActive(false);
        itemField.SetActive(false);
        ConnectPanel.SetActive(false);
    }
    public override void OnJoinedRoom()
    {
        LobbyPanel.SetActive(false);
        DisconnectPanel.SetActive(false);
        InGameUI.SetActive(true);
        ground.SetActive(true);
        itemField.SetActive(true);
        ConnectPanel.SetActive(true);
        //connecting = false;
    }

    public void CreateRoomButtonClickInPanel()
    {
        PhotonNetwork.CreateRoom(RoomNameInputField.text, new RoomOptions { MaxPlayers = maxPlayersPerRoom });        
        //PhotonNetwork.JoinRoom(RoomNameToCreat.text);
    }

    public void Spawn(string character)
    {
        ConnectPanel.SetActive(false);
        RespwanPanel.SetActive(false);

        GameObject player = PhotonNetwork.Instantiate("Character/" + character, Vector3.zero, Quaternion.identity);

        GameManager.Instance.setUpCharacter(player);
        if (PhotonNetwork.IsMasterClient)
            spawnButton.SetActive(true);

        InGameUI.GetComponent<InGameUI>().myCharacter = player;
        InGameUI.GetComponent<InGameUI>().setUp();
    }

    public void BossSpawnButtonClick()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Monster/Evil Wizard", Vector3.zero, Quaternion.identity);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PV.RPC("SpawnBoss", RpcTarget.All);
            StageManager.active = true;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespwanPanel.SetActive(false);
        InGameUI.SetActive(false);
        ground.SetActive(false);
    }

    [PunRPC]
    void SpawnBoss()
    {
        spawnButton.SetActive(false);
        InGameUI.GetComponent<InGameUI>().BossSetUp();
    }
}
