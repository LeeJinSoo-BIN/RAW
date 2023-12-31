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
using System;
using WebSocketSharp;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private byte maxPlayersPerRoom = 3;

    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespwanPanel;
    public GameObject ConnectPanel;

    public GameObject InGameUI;
    public GameObject PanelCanvas;
    public GameObject ground;
    public GameObject itemField;
    private bool connectedToMaster = false;
    private bool joinedToLobby = false;
    public TMP_Text ConnectButtonText;
    public PhotonView PV;
    public GameObject spawnButton;
    public GameObject timeLimit;

    public GameObject roomInfo;
    public GameObject RoomList;
    public GameObject LobbyPanel;
    public TMP_InputField RoomNameInputField;

    private Dictionary<string, RoomInfo> roomListDict = new Dictionary<string, RoomInfo>();

    private string chatLog;
    public TMP_InputField inGameChatInputField;
    public TMP_Text inGameChatBox;
    private bool chatEnd = false;

    public TMP_Text disconnectButtonText;
    public GameObject quitButton;

    private Dictionary<string, int> rollNum = new Dictionary<string, int>();
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
        InGameUI.transform.GetChild(0).gameObject.SetActive(false);
        InGameUI.transform.GetChild(1).gameObject.SetActive(false);
        for(int k = 0; k < PanelCanvas.transform.childCount; k++)
        {
            PanelCanvas.transform.GetChild(k).gameObject.SetActive(false);
        }
        ground.SetActive(false);
        itemField.SetActive(false);
        ConnectPanel.SetActive(false);
        DisconnectPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        inGameChatInputField.onSubmit.AddListener(delegate { sendChat(); });

        rollNum.Add("Sword", 0);
        rollNum.Add("Arrow", 1);
        rollNum.Add("Magic", 2);
    }
    private void Update()
    {
        if (!connectedToMaster)
        {
            ConnectButtonText.text = PhotonNetwork.NetworkClientState.ToString();
        }
        if(PhotonNetwork.InRoom)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!inGameChatInputField.isFocused && !chatEnd)
                {                    
                    inGameChatInputField.ActivateInputField();
                }
                chatEnd = false;
            }
        }            
    }    
    public void sendChat()
    {        
        if (inGameChatInputField.text != "")
        {
            string chat = inGameChatInputField.text;
            PV.RPC("sendChatLog", RpcTarget.All, PhotonNetwork.NickName + " : " + chat);            
            inGameChatInputField.text = "";            
            chatEnd = false;
        }
        else
        {
            inGameChatInputField.DeactivateInputField();
            chatEnd = true;
        }
    }

    void updateChatLog()
    {
        inGameChatBox.text = chatLog;
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        //connecting = true;
    }

    public void JoinLobby()
    {
        if (NickNameInput.text != "")
            PhotonNetwork.JoinLobby();
    }

    public void ClickConnectButton()
    {
        if (ConnectButtonText.text == "접속하기")
            JoinLobby();
        else if (ConnectButtonText.text == "로그인")
            Connect();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속");
        connectedToMaster = true;
        ConnectButtonText.text = "접속하기";//"?묒냽";
        disconnectButtonText.text = "로그아웃";
        quitButton.SetActive(true);
        if (joinedToLobby)
            JoinLobby();
        
    }    
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 접속");
        joinedToLobby = true;
        LobbyPanel.SetActive(true);
        DisconnectPanel.SetActive(false);
        disconnectButtonText.text = "접속 끊기";
        roomListDict.Clear();
        chatLog = "";
        updateChatLog();
    }
    public override void OnLeftLobby()
    {
        LobbyPanel.SetActive(false);
        DisconnectPanel.SetActive(true);
        disconnectButtonText.text = "로그아웃";
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
        ConnectPanel.transform.GetChild(1).gameObject.SetActive(true);
        ConnectPanel.transform.GetChild(2).gameObject.SetActive(true);
        ConnectPanel.transform.GetChild(3).gameObject.SetActive(true);
        InGameUI.transform.GetChild(0).gameObject.SetActive(true);
        InGameUI.transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        InGameUI.transform.GetChild(1).gameObject.SetActive(true);
        InGameUI.transform.GetChild(2).gameObject.SetActive(true);
        InGameUI.transform.GetChild(3).gameObject.SetActive(false);

        if(!PhotonNetwork.IsMasterClient)
        {
            InGameUI.transform.GetChild(1).gameObject.SetActive(false);
            InGameUI.transform.GetChild(2).gameObject.SetActive(false);
        }
        disconnectButtonText.text = "나가기";
        StageManager.stageTime = 0;
        StageManager.active = false;
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
        DataBase.Instance.selectedCharacterSpec = DataBase.Instance.defaultAccountInfo.characterList[rollNum[roll]];
        GameObject.Find("GameManager").GetComponent<GameManager>().setup(player);

        if (PhotonNetwork.IsMasterClient)
            spawnButton.SetActive(true);        
    }

    public void BossSpawnButtonClick()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (!timeLimit.GetComponent<TMP_InputField>().text.IsNullOrEmpty() && !int.TryParse(timeLimit.GetComponent<TMP_InputField>().text, out _))
                return;
            float limitTime;
            if (timeLimit.GetComponent<TMP_InputField>().text.IsNullOrEmpty())
                limitTime = 0;
            else
                limitTime = int.Parse(timeLimit.GetComponent<TMP_InputField>().text);
            StageManager.LimitTime = limitTime;
            PhotonNetwork.Instantiate("Monster/Evil Wizard", Vector3.zero, Quaternion.identity);
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PV.RPC("SpawnBoss", RpcTarget.All);            
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("서버 연결 끊김");
        DisconnectPanel.SetActive(true);
        RespwanPanel.SetActive(false);
        InGameUI.SetActive(false);
        ground.SetActive(false);
        disconnectButtonText.text = "게임 종료";
        ConnectButtonText.text = "로그인";
        quitButton.SetActive(false);
    }

    public void ClickDisconnectButton()
    {
        if (disconnectButtonText.text == "나가기")
            PhotonNetwork.LeaveRoom();
        else if (disconnectButtonText.text == "로그아웃")
            PhotonNetwork.Disconnect();
        else if (disconnectButtonText.text == "접속 끊기")
            PhotonNetwork.LeaveLobby();
        else if ( disconnectButtonText.text == "게임 종료")
            Application.Quit();
    }

    [PunRPC]
    void SpawnBoss()
    {
        spawnButton.SetActive(false);
        timeLimit.SetActive(false);
        StageManager.active = true;
        InGameUI.GetComponent<InGameUI>().BossSetUp();        
    }

    [PunRPC]
    void selectedRoll(string roll)
    {
        ConnectPanel.transform.Find(roll.ToUpper()).gameObject.SetActive(false);
    }

    [PunRPC]
    void sendChatLog(string chat)
    {
        chatLog += "\n" + chat;
        updateChatLog();
    }
}
