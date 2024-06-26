using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class newNetworkManager : MonoBehaviourPunCallbacks
{

    public PhotonView PV;
    public GameManager gameManager;
    public static newNetworkManager Instance;
    //public Dictionary<string, party> allPartys = new Dictionary<string, party>();
    //public HashSet<string> usersInParty = new HashSet<string>();


    public TMP_Text disconnectButtonText;
    private string nextRoomName;

    private void Awake()
    {
        /* var obj = FindObjectsOfType<newNetworkManager>();
         if (obj.Length == 1)
         {
             DontDestroyOnLoad(gameObject);
         }
         else
         {
             Destroy(gameObject);
         }*/
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        disconnectButtonText = UIManager.Instance.exitButtonText;
        //PV.RPC("UpdateParty", RpcTarget.All);
    }
    public override void OnJoinedRoom()
    {
        if (DataBase.Instance.currentMapType == "village")
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(DataBase.Instance.currentMapName);
            disconnectButtonText.text = "게임 나가기";            
        }
        else if (DataBase.Instance.currentMapType == "dungeon")
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel("Dungeon");
            disconnectButtonText.text = "던전 나가기";
            DataBase.Instance.isInDungeon = false;
        }
    }
    public override void OnConnectedToMaster()
    {
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            PhotonNetwork.JoinOrCreateRoom(nextRoomName, new RoomOptions { MaxPlayers = 3, PublishUserId = true }, null);
            DataBase.Instance.isInDungeon = true;
        }
        else if (DataBase.Instance.currentMapType == "village")
        {
            PhotonNetwork.JoinOrCreateRoom("Pallet Town", new RoomOptions { MaxPlayers = 20, PublishUserId = true }, null);
            DataBase.Instance.isInDungeon = false;
        }
        else if (DataBase.Instance.currentMapType == "character Select")
        {
            DataBase.Instance.currentMapName = "Login Scene";
            DataBase.Instance.isInDungeon = false;
            PhotonNetwork.JoinLobby();
        }
    }
    public override void OnJoinedLobby()
    {
        Destroy(UIManager.Instance.gameObject);
        SceneManager.LoadScene(DataBase.Instance.currentMapName);
    }


    public void ClickDisconnectButton()
    {
        if (disconnectButtonText.text == "던전 나가기")
        {
            DataBase.Instance.myPartyCaptainName = "";
            DataBase.Instance.myPartyName = "";
            DataBase.Instance.isCaptain = false;
            GoToVillage();
        }
        else if (disconnectButtonText.text == "게임 나가기")
        {
            DataBase.Instance.currentMapType = "character Select";
            DataBase.Instance.currentMapName = "Login Scene";
            UIManager.Instance.ClickLeavePartyButton();
            PhotonNetwork.LeaveRoom();
        }
        else if (disconnectButtonText.text == "RAW 종료")
            Application.Quit();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.Name == DataBase.Instance.myPartyCaptainName)
        {
            DataBase.Instance.myPartyMemNum--;
        }

        if (DataBase.Instance.currentMapType == "village" && PhotonNetwork.CurrentRoom.Name != DataBase.Instance.myPartyCaptainName && DataBase.Instance.myPartyCaptainName == otherPlayer.UserId)
        {
            UIManager.Instance.popInfo("파티장이 게임을 떠나 파티가 해체됩니다.");
            UIManager.Instance.ClickLeavePartyButton();
        }
    }

    public void movePortal(string dungeonName, int dungeonLevel)
    {
        UIManager.Instance.UpdatePartyPanel();
        int partyMemNum = UIManager.Instance.allPartys[DataBase.Instance.myCharacter.name].partyMembersNickName.Count;
        foreach (string mem in UIManager.Instance.allPartys[DataBase.Instance.myCharacter.name].partyMembersNickName)
        {
            if (mem == DataBase.Instance.myCharacter.name)
                continue;
            PV.RPC("enterDungeon", UIManager.Instance.inGameUserList[mem].PV.Owner, dungeonName, dungeonLevel, partyMemNum);
        }
        enterDungeon(dungeonName, dungeonLevel, partyMemNum);
    }

    #region 던전
    [PunRPC]
    void enterDungeon(string dungeonName, int dungeonLevel, int partyMemNum)
    {
        DataBase.Instance.myPartyMemNum = partyMemNum;
        UIManager.Instance.LoadingPop();
        DataBase.Instance.currentMapType = "dungeon";
        DataBase.Instance.currentMapName = dungeonName;
        DataBase.Instance.currentDungeonLevel = dungeonLevel;
        DataBase.Instance.currentStage = 1;        
        nextRoomName = DataBase.Instance.myPartyCaptainName;
        PhotonNetwork.LeaveRoom();        
    }

    [PunRPC]
    void ReGame()
    {
        UIManager.Instance.gameOverPanel.SetActive(false);
        UIManager.Instance.gameClearPanel.SetActive(false);
        DataBase.Instance.currentStage = 1;
        gameManager.isClearingMonster = true;
        gameManager.ReGame(false);
    }

    [PunRPC]
    void NextStage()
    {
        DataBase.Instance.currentStage++;
        gameManager.isClearingMonster = true;
        gameManager.ReGame(true);
    }

    [PunRPC]
    void GoToVillage()
    {
        UIManager.Instance.gameOverPanel.SetActive(false);
        UIManager.Instance.gameClearPanel.SetActive(false);
        DataBase.Instance.currentMapType = "village";
        DataBase.Instance.currentMapName = "Pallet Town";
        gameManager.isClearingMonster = true;
        Debug.Log("GoToVillage -> isClearing = true");
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void startTimer()
    {
        UIManager.Instance.timer = UIManager.Instance.startTimer();
        UIManager.Instance.StartCoroutine(UIManager.Instance.timer);
    }

    [PunRPC]
    void SpawnBoss()
    {
        //spawnButton.SetActive(false);
        //timeLimit.SetActive(false);
        //StageManager.active = true;
        UIManager.Instance.BossSetUp();
    }
    #endregion

    [PunRPC]
    void sendChatLog(string chat)
    {
        UIManager.Instance.chatLog += "\n" + chat;
        UIManager.Instance.updateChatLog();
    }

    [PunRPC]
    void sendInfo(string content)
    {
        UIManager.Instance.popInfo(content);
    }
    #region 파티

    [PunRPC]
    void sendAndReceiveInviteParty(string partyName, string fromWho)
    {
        UIManager.Instance.UpdatePartyPanel();
        UIManager.Instance.receiveInvite(partyName, fromWho);        
    }
    [PunRPC]
    void sendAndReceiveJoinRequestParty(string fromWho)
    {
        UIManager.Instance.UpdatePartyPanel();
        UIManager.Instance.receiveJoinRequest(fromWho);        
    }
    [PunRPC]
    void acceptJoinParty(string captain_name, string party_name)
    {
        DataBase.Instance.isCaptain = false;
        DataBase.Instance.myPartyCaptainName = captain_name;
        DataBase.Instance.myPartyName = party_name;
        DataBase.Instance.myCharacterState.updateParty();
        PV.RPC("UpdateParty", RpcTarget.All);
    }    

    [PunRPC]
    void kickPartyMember()
    {
        DataBase.Instance.isCaptain = false;
        DataBase.Instance.myPartyCaptainName = "";
        DataBase.Instance.myPartyName = "";
        DataBase.Instance.myCharacterState.updateParty();
        UIManager.Instance.popInfo("파티에서 강퇴당하였습니다.");
        PV.RPC("UpdateParty", RpcTarget.All);
    }

    [PunRPC]
    void ChangeCaptain(string newCaptainName)
    {
        if(newCaptainName == DataBase.Instance.myCharacter.name)
            DataBase.Instance.isCaptain = true;
        DataBase.Instance.myPartyCaptainName = newCaptainName;
        DataBase.Instance.myCharacterState.updateParty();        
    }

    [PunRPC]
    void UpdateParty()
    {
        UIManager.Instance.UpdatePartyPanel();
    }    
    #endregion

    #region 교환
    [PunRPC]
    void sendAndReceiveTradeRequest(string fromWho)
    {
        UIManager.Instance.receiveTradeRequest(fromWho);
    }

    [PunRPC]
    void upTradeItem(string itemName, int cnt, int slotPos, int enchant)
    {
        
        UIManager.Instance.UpdateOpTradeItem(itemName, cnt, slotPos, enchant);
    }

    [PunRPC]
    void leaveOrRejectTradePanel(bool reject)
    {
        UIManager.Instance.OpLeaveTrade(reject);
    }

    [PunRPC]
    void joinTradePanel()
    {
        UIManager.Instance.OpJoinTrade();        
    }

    [PunRPC]
    void sendTradeChatLog(string chat)
    {
        UIManager.Instance.tradeChatLog += "\n" + chat;
        UIManager.Instance.tradeChatLogShow.text = UIManager.Instance.tradeChatLog;
    }

    [PunRPC]
    void acceptTrade()
    {
        UIManager.Instance.OpAcceptTrade();
    }

    [PunRPC]
    void tradeDone(int type)
    {
        UIManager.Instance.TradeDone(type);
    }
    [PunRPC]
    void tryTrade(bool iCan)
    {
        UIManager.Instance.tryTrade(iCan);
    }
    #endregion
}


