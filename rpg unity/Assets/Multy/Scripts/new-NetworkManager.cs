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

    public Dictionary<string, party> allPartys = new Dictionary<string, party>();
    public HashSet<string> captainsList = new HashSet<string>();
    public string myPartyCaptainName;
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


    [PunRPC]
    void sendChatLog(string chat)
    {
        chatLog += "\n" + chat;
        updateChatLog();
    }


    [PunRPC]
    void sendAndRecieveInviteParty(string partyName, string fromWho)
    {
        UIManager.recieveInvite(partyName, fromWho);
    }
    [PunRPC]
    void sendAndRecieveJoinRequestParty(string fromWho)
    {
        UIManager.recieveJoinRequest(fromWho);
    }

    [PunRPC]
    void registParty(string partyName, string captain)
    {        
        if(!allPartys.ContainsKey(captain) && !captainsList.Contains(captain))
        {
            party newParty;
            newParty.partyName = partyName;
            newParty.partyMembersNickName = new HashSet<string> { captain };
            allPartys.Add(captain, newParty);
            captainsList.Add(captain);
        }
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void joinParty(string captainName, string member)
    {
        allPartys[captainName].partyMembersNickName.Add(member);
        UIManager.UpdatePartyPanel();
    }

    [PunRPC]
    void LeaveParty(string captainName, string leaveName)
    {
        allPartys[captainName].partyMembersNickName.Remove(leaveName);        
    }

    [PunRPC]
    void ChangeCaptain(string captainName, string newCaptainName, bool exit)
    {
        captainsList.Remove(captainName);
        captainsList.Add(newCaptainName);
        allPartys.Add(newCaptainName, allPartys[captainName]);
        allPartys.Remove(captainName);
        if (exit)
        {
            allPartys[newCaptainName].partyMembersNickName.Remove(captainName);
        }
    }

    [PunRPC]
    void BoomParty(string captainName)
    {
        allPartys.Remove(captainName);
        captainsList.Remove(captainName);
        if (myPartyCaptainName == captainName)
            myPartyCaptainName = "";

    }
}
