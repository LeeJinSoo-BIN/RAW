using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using System.Data;
using MySql.Data.MySqlClient;

public class Login : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public TMP_InputField pwInputField;
    public TMP_InputField pwCheckInputField;
    public TMP_InputField idInputField;
    private int maxNumServerPlayer = 20;
    public TMP_Text currentNumOnlinePlayer;
    private string selectedCharacterName;
    public GameObject characterSelectList;
    public GameObject characterSelectButton;
       

    public GameObject LoginPanel;
    public GameObject SelectCharacterPanel;
    public Button LoginButton;

    public GameObject StatusPop;
    public TMP_Text connectStatusText;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        Application.targetFrameRate = 60;
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        LoginPanel.SetActive(true);
        SelectCharacterPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (pwInputField.isFocused || pwCheckInputField.isFocused) // password ?? ???????? ??????????
        {
            Input.imeCompositionMode = IMECompositionMode.Off;
        }
        else if (idInputField.isFocused)
        {            
            //Input.imeCompositionMode = IMECompositionMode.On;
        }
        else
            Input.imeCompositionMode = IMECompositionMode.Auto;

        

    }

    public void Show_Hide_PassWordCheck(bool show)
    {
        if (pwCheckInputField.gameObject.activeSelf == show)
            return;
        if (show)
        {
            idInputField.transform.localPosition = new Vector3(idInputField.transform.localPosition.x, idInputField.transform.localPosition.y + 95, 0);
            pwInputField.transform.localPosition = new Vector3(pwInputField.transform.localPosition.x, pwInputField.transform.localPosition.y + 95, 0);
            pwCheckInputField.gameObject.SetActive(show);
        }
        else
        {
            idInputField.transform.localPosition = new Vector3(idInputField.transform.localPosition.x, idInputField.transform.localPosition.y - 95, 0);
            pwInputField.transform.localPosition = new Vector3(pwInputField.transform.localPosition.x, pwInputField.transform.localPosition.y - 95, 0);
            pwCheckInputField.gameObject.SetActive(show);
        }
    }

    public void RegisterAccount()
    {
        if (pwCheckInputField.gameObject.activeSelf)
        {
            if (pwCheckInputField.text == pwInputField.text)
                Debug.Log("account");
            else
            {
                Debug.Log("wrong");
            }
        }
    }
    IEnumerator LoginMessageUpdate()
    {
        while (true)
        {
            connectStatusText.text = PhotonNetwork.NetworkClientState.ToString();
            if (PhotonNetwork.IsConnected)
                break;
            yield return null;
        }
    }

    public void OnLoginButtonClicked()
    {
        if (!pwCheckInputField.gameObject.activeSelf)
        {
            if (!idInputField.text.IsNullOrEmpty() && !pwInputField.text.IsNullOrEmpty())
            {
                try
                {
                    MySqlConnection conn = DBControl.SqlConn;

                    conn.Open();

                    int loginStatus = 0;

                    string loginId = idInputField.text;
                    string loginPw = pwInputField.text;

                    string selectQuery = "SELECT * FROM account WHERE name = \'" + loginId + "\' ";

                    MySqlCommand selectCommand = new MySqlCommand(selectQuery, conn);
                    MySqlDataReader userAccount = selectCommand.ExecuteReader();

                    while (userAccount.Read())
                    {
                        if (loginId == (string)userAccount["name"] && loginPw == (string)userAccount["password"])
                        {
                            loginStatus = 1;
                        }
                    }

                    conn.Close();

                    if (loginStatus == 1)
                    {
                        Debug.Log("Success");

                        LoginButton.enabled = false;
                        StatusPop.SetActive(true);
                        StartCoroutine(LoginMessageUpdate());
                        PhotonNetwork.ConnectUsingSettings();
                    }
                    else
                    {
                        Debug.Log("Fail");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
        
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        StatusPop.SetActive(false);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        LoginButton.enabled = true;
    }

    public override void OnJoinedLobby()
    {
        LoginPanel.SetActive(false);
        updateCharacterList();
        SelectCharacterPanel.SetActive(true);        
    }
    public void ClickChannel()
    {
        string channelName = EventSystem.current.currentSelectedGameObject.name;
        PhotonNetwork.JoinOrCreateRoom(channelName, new RoomOptions { MaxPlayers = maxNumServerPlayer }, null);
    }
    public void IME_Off()
    {
        Debug.Log("Off");
        Input.imeCompositionMode = IMECompositionMode.Off;
    }
    public void IME_On()
    {
        Debug.Log("On");
        Input.imeCompositionMode = IMECompositionMode.On;
    }

    public void IME_Auto()
    {
        Debug.Log("Auto");
        Input.imeCompositionMode = IMECompositionMode.Auto;
    }

    void updatePlayerCount()
    {
        currentNumOnlinePlayer.text = PhotonNetwork.CountOfPlayersInRooms.ToString() + " / " + maxNumServerPlayer.ToString();
    }
    void updateCharacterList()
    {
        for(int k = 0; k < DataBase.Instance.defaultAccountInfo.characterList.Count; k++)
        {
            GameObject characterButton = Instantiate(characterSelectButton);
            List<InventoryItem> equipment = DataBase.Instance.defaultAccountInfo.characterList[k].equipment;
            SPUM_SpriteList spriteList = characterButton.transform.GetChild(0).GetComponentInChildren<SPUM_SpriteList>();
            foreach (InventoryItem item in equipment)
            {
                string current_item_sprite = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;
                spriteList.PartsPath[DataBase.Instance.itemInfoDict[item.itemName].itemType] = current_item_sprite;
                //Debug.Log(spriteList.PartsPath[itemInfoDict[item.itemName].itemType]);
            }
            spriteList._hairAndEyeColor = DataBase.Instance.defaultAccountInfo.characterList[k].colors;
            spriteList.setSprite();

            characterButton.transform.GetChild(1).name = k.ToString();
            characterButton.transform.GetChild(2).GetComponent<TMP_Text>().text = DataBase.Instance.defaultAccountInfo.characterList[k].nickName;

            characterButton.SetActive(true);
            characterButton.transform.parent = characterSelectList.transform;
            characterButton.transform.localPosition = Vector3.zero;
            characterButton.transform.localScale = Vector3.one;
        }
    }

    public override void OnJoinedRoom()
    {        
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(DataBase.Instance.currentMapName);
    }
        public void ClickCharacterSelectButton()
    {
        int whichCharacter = int.Parse(EventSystem.current.currentSelectedGameObject.name);
        PhotonNetwork.JoinOrCreateRoom("palletTown", new RoomOptions { MaxPlayers = maxNumServerPlayer }, null);
        DataBase.Instance.selectedCharacterSpec = DataBase.Instance.defaultAccountInfo.characterList[whichCharacter];
        DataBase.Instance.currentMapName = DataBase.Instance.defaultAccountInfo.characterList[whichCharacter].lastTown;
        PhotonNetwork.NickName = DataBase.Instance.selectedCharacterSpec.nickName;
    }
}
