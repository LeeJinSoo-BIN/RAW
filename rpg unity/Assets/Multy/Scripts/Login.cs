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

    public GameObject PopPanel;
    public TMP_Text popTitle;
    public TMP_Text popContent;

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
            {
                try
                {
                    MySqlConnection conn = DBControl.SqlConn;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    string loginId = idInputField.text;
                    string loginPw = pwInputField.text;

                    string insertQuery = "INSERT INTO account (name, password) VALUES (\'" + loginId + "\', \'" + loginPw + "\'); ";

                    MySqlCommand command = new MySqlCommand(insertQuery, conn);

                    if (command.ExecuteNonQuery() == 1)
                    {
                        Debug.Log("register success");
                        StartCoroutine(popMessage("계정 생성 성공", "로그인 해주세요."));
                    }
                    else
                    {
                        Debug.Log("fail");
                        StartCoroutine(popMessage("계정 생성 실패", "서버 오류"));
                    }
                    if (conn.State == ConnectionState.Open)
                        conn.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    if (e.Message.Contains("Duplicate"))
                    {
                        StartCoroutine(popMessage("계정 생성 실패", "이미 등록된 아이디 입니다."));
                    }
                    else
                        StartCoroutine(popMessage("서버 오류", e.Message));
                }
            }
            else
            {
                Debug.Log("wrong");
                StartCoroutine(popMessage("오류", "비밀번호가 일치하지 않습니다."));
            }
        }
    }

    public void ConnectWithOutLogin()
    {
        LoginButton.enabled = false;
        PopPanel.SetActive(true);
        StartCoroutine(LoginMessageUpdate());
        PhotonNetwork.ConnectUsingSettings();
    }
    public void ClickLoginButton()
    {
        string loginId = idInputField.text;
        string loginPw = pwInputField.text;

        if (!pwCheckInputField.gameObject.activeSelf)
        {
            if (!loginId.IsNullOrEmpty() && !loginPw.IsNullOrEmpty())
            {
                try
                {
                    MySqlConnection conn = DBControl.SqlConn;
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    int loginStatus = 0;

                    string selectQuery = "SELECT * FROM account WHERE name = \'" + loginId + "\' ";

                    MySqlCommand selectCommand = new MySqlCommand(selectQuery, conn);
                    MySqlDataReader userAccount = selectCommand.ExecuteReader();

                    while (userAccount.Read())
                    {
                        if (loginId == (string)userAccount["name"] && loginPw == (string)userAccount["password"])
                        {
                            loginStatus = 1;
                            break;
                        }
                    }
                    if(conn.State == ConnectionState.Open)
                        conn.Close();

                    if (loginStatus == 1)
                    {
                        Debug.Log("Success");

                        LoginButton.enabled = false;
                        PopPanel.SetActive(true);
                        StartCoroutine(LoginMessageUpdate());
                        PhotonNetwork.ConnectUsingSettings();
                    }
                    else
                    {
                        Debug.Log("Fail");
                        StartCoroutine(popMessage("로그인 실패", "아이디와 비밀번호를 확인해주세요."));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    StartCoroutine(popMessage("서버 오류", e.Message));
                }
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PopPanel.SetActive(false);
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
        DataBase.Instance.currentMapType = "village";
        PhotonNetwork.NickName = DataBase.Instance.selectedCharacterSpec.nickName;
    }

    IEnumerator popMessage(string title, string content, float popTime = 2f)
    {
        popTitle.text = title;
        popContent.text = content;
        PopPanel.SetActive(true);
        float _time = 0f;
        while(_time < popTime)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        PopPanel.SetActive(false);
    }
    IEnumerator LoginMessageUpdate()
    {
        popTitle.text = "접속중";
        while (true)
        {
            popContent.text = PhotonNetwork.NetworkClientState.ToString();
            if (PhotonNetwork.IsConnected)
                break;
            yield return null;
        }
    }

}
