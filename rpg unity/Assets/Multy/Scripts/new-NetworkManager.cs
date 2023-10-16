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

    public void InviteParty(Player reciver, string ment)
    {
        PV.RPC("sendAndRecieveInviteParty", reciver, ment);
    }
    [PunRPC]
    void sendChatLog(string chat)
    {
        chatLog += "\n" + chat;
        updateChatLog();
    }


    [PunRPC]
    void sendAndRecieveInviteParty(string ment)
    {
        UIManager.recieveInvite(ment);
    }

    [PunRPC]
    void acceptOrRejectInviteParty(bool accept)
    {
        if (accept)
        {

        }
    }
}
