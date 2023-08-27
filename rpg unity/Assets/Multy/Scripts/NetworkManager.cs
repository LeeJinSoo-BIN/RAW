using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespwanPanel;
    public GameObject ConnectPanel;
    
    public GameObject InGameUI;
    public GameObject ground;
    private bool connecting = false;
    public TMP_Text ConnectButtonText;
    public PhotonView PV;
    public GameObject spawnButton;
    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        InGameUI.SetActive(false);
        ground.SetActive(false);
        
        ConnectPanel.SetActive(false);
        DisconnectPanel.SetActive(true);        
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        connecting = true;
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }

    public override void OnCreatedRoom()
    {        
        
    }

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        InGameUI.SetActive(false);
        ground.SetActive(false);
        ConnectPanel.SetActive(true);
        connecting = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        if (connecting)
        {
            ConnectButtonText.text = PhotonNetwork.NetworkClientState.ToString();
        }
    }

    public void Spawn(string character)
    {
        GameObject player = PhotonNetwork.Instantiate("Character/" + character, Vector3.zero, Quaternion.identity);
        
        ConnectPanel.SetActive(false);
        RespwanPanel.SetActive(false);
        InGameUI.SetActive(true);
        ground.SetActive(true);
        InGameUI.GetComponent<InGameUI>().myCharacter = player;
        InGameUI.GetComponent<InGameUI>().setUp();
        
    }
    
    public void BossSpawnButtonClick()
    {
        Debug.Log("clicekd");
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Evil Wizard", Vector3.zero, Quaternion.identity);
            PV.RPC("SpawnBoss", RpcTarget.All);
        }
    }

    [PunRPC]
    void SpawnBoss()
    {   
        spawnButton.SetActive(false);
        InGameUI.GetComponent<InGameUI>().BossSetUp();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespwanPanel.SetActive(false);
        InGameUI.SetActive(false);
        ground.SetActive(false);
    }
}