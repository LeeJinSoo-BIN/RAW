using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class newNetworkManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField inGameChatInputField;
    public TMP_Text inGameChatBox;
    private bool chatEnd = false;
    private string chatLog = "";

    public PhotonView PV;

    public UIManager UIManager;
    public GameManager GameManager;

    public Dictionary<string, party> allPartys = new Dictionary<string, party>();
    public HashSet<string> usersInParty = new HashSet<string>();
    public string myPartyCaptainName;

    public TMP_Text disconnectButtonText;
    private bool goToDungeon;
    private string nextRoom;
    public struct party
    {
        public string partyName;
        public HashSet<string> partyMembersNickName;
    }


    void Start()
    {
        inGameChatInputField.onSubmit.AddListener(delegate { sendChat(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom)
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


    public override void OnJoinedRoom()
    {
        if(DataBase.Instance.currentMapType == "village")
        {

        }
        else if(DataBase.Instance.currentMapType == "dungeon")
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(DataBase.Instance.currentMapName);
        }
    }
    public override void OnConnectedToMaster()
    {
        if (goToDungeon)
        {
            PhotonNetwork.JoinOrCreateRoom(nextRoom, new RoomOptions { MaxPlayers = 3 }, null);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.ActorNumber);
        if (DataBase.Instance.currentMapType == "village")
        {
            allPartys.Remove(UIManager.idToNickName[otherPlayer.ActorNumber]);
            usersInParty.Remove(UIManager.idToNickName[otherPlayer.ActorNumber]);
            if (myPartyCaptainName == UIManager.idToNickName[otherPlayer.ActorNumber])
                myPartyCaptainName = "";
            UIManager.UpdatePartyPanel();
        }
    }
    
    public void ClickDisconnectButton()
    {
        if (disconnectButtonText.text == "나가기")
            PhotonNetwork.LeaveRoom();
        else if (disconnectButtonText.text == "로그아웃")
            PhotonNetwork.Disconnect();
        else if (disconnectButtonText.text == "접속 끊기")
            PhotonNetwork.LeaveLobby();
        else if (disconnectButtonText.text == "게임 종료")
            Application.Quit();
    }

    public void movePortal(float timeLimit)
    {
        foreach(string mem in allPartys[DataBase.Instance.currentCharacterNickname].partyMembersNickName)
        {
            if (mem == DataBase.Instance.currentCharacterNickname)
                continue;
            PV.RPC("enterDungeon", UIManager.inGameUserList[mem], timeLimit);
        }
        enterDungeon(timeLimit);
    }


    [PunRPC]
    void enterDungeon(float timeLimit)
    {
        DataBase.Instance.currentMapType = "dungeon";
        DataBase.Instance.currentMapName = "Dungeon";
        StageManager.LimitTime = timeLimit;
        nextRoom = myPartyCaptainName;
        goToDungeon = true;
        PhotonNetwork.LeaveRoom();        
    }

    [PunRPC]
    void sendChatLog(string chat)
    {
        chatLog += "\n" + chat;
        updateChatLog();
    }


    [PunRPC]
    void sendAndReceiveInviteParty(string partyName, string fromWho)
    {
        UIManager.receiveInvite(partyName, fromWho);
        UIManager.UpdatePartyPanel();
    }
    [PunRPC]
    void sendAndReceiveJoinRequestParty(string fromWho)
    {
        UIManager.receiveJoinRequest(fromWho);
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void kickPartyMember(string captain, string who)
    {
        allPartys[captain].partyMembersNickName.Remove(who);
        if(who == DataBase.Instance.currentCharacterNickname)
        {
            myPartyCaptainName = "";
        }
        usersInParty.Remove(who);
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void registParty(string partyName, string captain)
    {
        if (!allPartys.ContainsKey(captain))
        {
            party newParty;
            newParty.partyName = partyName;
            newParty.partyMembersNickName = new HashSet<string> { captain };
            allPartys.Add(captain, newParty);
            usersInParty.Add(captain);
        }
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void joinParty(string captainName, string member)
    {
        allPartys[captainName].partyMembersNickName.Add(member);
        if (member == DataBase.Instance.currentCharacterNickname)
            myPartyCaptainName = captainName;
        usersInParty.Add(member);
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void LeaveParty(string captainName, string leaveName)
    {
        allPartys[captainName].partyMembersNickName.Remove(leaveName);
        if (leaveName == DataBase.Instance.currentCharacterNickname)
            myPartyCaptainName = "";
        usersInParty.Remove(leaveName);
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void ChangeCaptain(string captainName, string newCaptainName, bool exit)
    {
        party newParty = allPartys[captainName];
        allPartys.Add(newCaptainName, newParty);
        allPartys.Remove(captainName);
        if (myPartyCaptainName == captainName)
            myPartyCaptainName = newCaptainName;
        if (captainName == DataBase.Instance.currentCharacterNickname)
            myPartyCaptainName = "";
        if (exit)
        {
            allPartys[newCaptainName].partyMembersNickName.Remove(captainName);
            usersInParty.Remove(captainName);
        }
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void BoomParty(string captainName)
    {
        allPartys.Remove(captainName);
        usersInParty.Remove(captainName);
        if (myPartyCaptainName == captainName)
            myPartyCaptainName = "";
        UIManager.UpdatePartyPanel();
    }



    [PunRPC]
    void SpawnBoss()
    {
        //spawnButton.SetActive(false);
        //timeLimit.SetActive(false);
        StageManager.active = true;
        GameManager.inGameUI.GetComponent<InGameUI>().BossSetUp();
    }
}
