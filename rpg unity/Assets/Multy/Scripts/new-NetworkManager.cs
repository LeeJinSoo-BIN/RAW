using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class newNetworkManager : MonoBehaviourPunCallbacks
{

    public PhotonView PV;
    public GameManager gameManager;

    public Dictionary<string, party> allPartys = new Dictionary<string, party>();
    public HashSet<string> usersInParty = new HashSet<string>();
    public string myPartyCaptainName;

    public TMP_Text disconnectButtonText;
    private string nextRoom;
    public struct party
    {
        public string partyName;
        public HashSet<string> partyMembersNickName;
    }    

    void Awake()
    {
        allPartys.Clear();
        usersInParty.Clear();
        myPartyCaptainName = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnJoinedRoom()
    {
        if(DataBase.Instance.currentMapType == "village")
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(DataBase.Instance.currentMapName);
        }
        else if(DataBase.Instance.currentMapType == "dungeon")
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(DataBase.Instance.currentMapName);
        }
    }
    public override void OnConnectedToMaster()
    {
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            PhotonNetwork.JoinOrCreateRoom(nextRoom, new RoomOptions { MaxPlayers = 3 }, null);            
        }
        else if(DataBase.Instance.currentMapType == "village")
        {
            PhotonNetwork.JoinOrCreateRoom("palletTown", new RoomOptions { MaxPlayers = 20 }, null);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.ActorNumber);
        if (DataBase.Instance.currentMapType == "village")
        {
            if (UIManager.Instance.idToNickName.ContainsKey(otherPlayer.ActorNumber))
            {
                allPartys.Remove(UIManager.Instance.idToNickName[otherPlayer.ActorNumber]);
                usersInParty.Remove(UIManager.Instance.idToNickName[otherPlayer.ActorNumber]);
                if (myPartyCaptainName == UIManager.Instance.idToNickName[otherPlayer.ActorNumber])
                    myPartyCaptainName = "";
                UIManager.Instance.UpdatePartyPanel();
            }
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
            PV.RPC("enterDungeon", UIManager.Instance.inGameUserList[mem], timeLimit);
        }
        enterDungeon(timeLimit);
        DataBase.Instance.isCurrentDungeonCaptain = true;
    }


    [PunRPC]
    void enterDungeon(float timeLimit)
    {
        DataBase.Instance.isCurrentDungeonCaptain = false;
        DataBase.Instance.currentMapType = "dungeon";
        DataBase.Instance.currentMapName = "Dungeon";
        UIManager.Instance.limitTime = timeLimit;
        nextRoom = myPartyCaptainName;
        PhotonNetwork.LeaveRoom();        
    }

    [PunRPC]
    void ReGame()
    {
        UIManager.Instance.gameOverPanel.SetActive(false);
        gameManager.ReGame();
    }

    [PunRPC]
    void GoToVillage()
    {
        UIManager.Instance.gameOverPanel.SetActive(false);
        DataBase.Instance.isCurrentDungeonCaptain = false;
        DataBase.Instance.currentMapType = "village";
        DataBase.Instance.currentMapName = "Pallet Town";
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void sendChatLog(string chat)
    {
        UIManager.Instance.chatLog += "\n" + chat;
        UIManager.Instance.updateChatLog();
    }


    [PunRPC]
    void sendAndReceiveInviteParty(string partyName, string fromWho)
    {
        UIManager.Instance.receiveInvite(partyName, fromWho);
        UIManager.Instance.UpdatePartyPanel();
    }
    [PunRPC]
    void sendAndReceiveJoinRequestParty(string fromWho)
    {
        UIManager.Instance.receiveJoinRequest(fromWho);
        UIManager.Instance.UpdatePartyPanel();
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
        UIManager.Instance.UpdatePartyPanel();
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
        UIManager.Instance.UpdatePartyPanel();
    }

    [PunRPC]
    void joinParty(string captainName, string member)
    {
        allPartys[captainName].partyMembersNickName.Add(member);
        if (member == DataBase.Instance.currentCharacterNickname)
            myPartyCaptainName = captainName;
        usersInParty.Add(member);
        UIManager.Instance.UpdatePartyPanel();
    }

    [PunRPC]
    void LeaveParty(string captainName, string leaveName)
    {
        allPartys[captainName].partyMembersNickName.Remove(leaveName);
        if (leaveName == DataBase.Instance.currentCharacterNickname)
            myPartyCaptainName = "";
        usersInParty.Remove(leaveName);
        UIManager.Instance.UpdatePartyPanel();
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
        UIManager.Instance.UpdatePartyPanel();
    }

    [PunRPC]
    void BoomParty(string captainName)
    {
        allPartys.Remove(captainName);
        usersInParty.Remove(captainName);
        if (myPartyCaptainName == captainName)
            myPartyCaptainName = "";
        UIManager.Instance.UpdatePartyPanel();
    }



    [PunRPC]
    void SpawnBoss()
    {
        //spawnButton.SetActive(false);
        //timeLimit.SetActive(false);
        StageManager.active = true;
        UIManager.Instance.BossSetUp();        
    }
}
