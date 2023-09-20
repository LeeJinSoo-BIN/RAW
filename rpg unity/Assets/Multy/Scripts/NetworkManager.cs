using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.EventSystems;
using Photon.Pun.Demo.Cockpit;

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
    private bool connectedToMaster = false;
    private bool joinedToLobby = false;
    public TMP_Text ConnectButtonText;
    public PhotonView PV;
    public GameObject spawnButton;

    public GameObject roomInfo;
    public GameObject RoomList;
    public GameObject LobbyPanel;
    public TMP_InputField RoomNameInputField;

    private Dictionary<string, RoomInfo> roomListDict = new Dictionary<string, RoomInfo>();

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
        if (!connectedToMaster)
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
        PhotonNetwork.JoinLobby();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속");
        connectedToMaster = true;
        ConnectButtonText.text = "접속하기";//"?묒냽";
        if (joinedToLobby)
            JoinLobby();
        
    }    
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 접속");
        joinedToLobby = true;
        LobbyPanel.SetActive(true);
        DisconnectPanel.SetActive(false);
        roomListDict.Clear();
    }
    public override void OnLeftLobby()
    {
        Debug.Log("로비 접속 끊김");
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("room list updated");        
        foreach(RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                roomListDict.Remove(room.Name);
            }
            else
            {
                roomListDict[room.Name] = room;
            }            
        }
        UpdateRoomList();
    }

    void UpdateRoomList()
    { 
        for (int k = 0; k < RoomList.transform.childCount; k++)
        {
            Destroy(RoomList.transform.GetChild(k).gameObject);
        }        
        foreach (string room in roomListDict.Keys)
        {            
            GameObject Room = Instantiate(roomInfo);
            Room.transform.SetParent(RoomList.transform);
            Room.transform.localScale = new Vector3(1, 1, 1);
            Room.SetActive(true);
            Room.transform.GetChild(0).GetComponent<TMP_Text>().text = roomListDict[room].Name;
            string count = roomListDict[room].PlayerCount + " / " + roomListDict[room].MaxPlayers;
            Room.transform.GetChild(1).GetComponent<TMP_Text>().text = count;
            if (roomListDict[room].PlayerCount == roomListDict[room].MaxPlayers || !roomListDict[room].IsOpen)
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
        ConnectPanel.transform.GetChild(1).gameObject.SetActive(true);
        ConnectPanel.transform.GetChild(2).gameObject.SetActive(true);
        ConnectPanel.transform.GetChild(3).gameObject.SetActive(true);
        InGameUI.transform.GetChild(1).GetChild(4).gameObject.SetActive(false);
    }    

    public void CreateRoomButtonClickInPanel()
    {
        PhotonNetwork.CreateRoom(RoomNameInputField.text, new RoomOptions { MaxPlayers = maxPlayersPerRoom });        
        //PhotonNetwork.JoinRoom(RoomNameToCreat.text);
    }

    public void Spawn(string roll)
    {
        PV.RPC("selectedRoll", RpcTarget.AllBuffered, roll);
        ConnectPanel.SetActive(false);
        RespwanPanel.SetActive(false);
        GameObject player = PhotonNetwork.Instantiate("Character/Player", Vector3.zero, Quaternion.identity);
        GameManager.Instance.setup(player, roll);

        if (PhotonNetwork.IsMasterClient)
            spawnButton.SetActive(true);        
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
        Debug.Log("서버 연결 끊김");
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

    [PunRPC]
    void selectedRoll(string roll)
    {
        ConnectPanel.transform.Find(roll.ToUpper()).gameObject.SetActive(false);
    }
}
