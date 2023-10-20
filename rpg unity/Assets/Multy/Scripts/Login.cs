using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    //public TMP_Text currentNumOnlinePlayer;
    private string selectedCharacterName;
    public GameObject characterSelectList;
    public GameObject characterSelectButton;

    public GameObject LoginPanel;
    public GameObject SelectCharacterPanel;
    public GameObject CharacterCreatePanel;
    public Button LoginButton;

    public GameObject PopPanel;
    public TMP_Text popTitle;
    public TMP_Text popContent;

    public List<string> rollList = new List<string>();
    public List<InventoryItem> defaultHair = new List<InventoryItem>();
    public List<InventoryItem> defaultCloth = new List<InventoryItem>();
    public List<InventoryItem> defaultWeapon = new List<InventoryItem>();
    public List<CharacterSpec> defaultCharacterSpec = new List<CharacterSpec>();

    public SPUM_SpriteList CreateCharacterSample;
    public TMP_Text CreateCharacterRollText;
    public TMP_InputField CreatCharacterNickInput;
    public int currentRollIdx = 0;
    public int currentHairIdx = 0;
    public int currentClothIdx = 0;
    

    private Color currentHairColor = new Color(113f / 255f, 38f / 255f, 38f / 255f);
    private Color currentEyeColor = new Color(113f / 255f, 38f / 255f, 38f / 255f);

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
        CharacterCreatePanel.SetActive(false);
    }

    void Update()
    {
        if (LoginPanel.activeSelf)
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

    private bool CheckDuplicateNickName(string nickName)
    {
        bool duplicated = true;

        return duplicated;
    }

    public void CreateNewCharacter(CharacterSpec newSpec)
    {
        string ID = DataBase.Instance.defaultAccountInfo.accountId;
        string newNickName = newSpec.nickName;


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
                        DataBase.Instance.defaultAccountInfo.accountId = loginId;
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
    public void ConnectWithOutLogin()
    {
        LoginButton.enabled = false;
        PopPanel.SetActive(true);
        StartCoroutine(LoginMessageUpdate());
        PhotonNetwork.ConnectUsingSettings();
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
    public void ClickCreateNewCharacterButton()
    {
        CharacterCreatePanel.SetActive(true);       

        UpdateSample("hair");
        UpdateSample("cloth");
        UpdateSample("roll");

    }

    public void ClickCheckDuplicatedNickButton()
    {
        bool duplicated = CheckDuplicateNickName(CreatCharacterNickInput.text);
        if (duplicated)
        {
            StartCoroutine(popMessage("중복", "중복된 닉네임 입니다."));
        }
    }


    void UpdateSample(string part)
    {
        string part_ = null;
        string path = null;
        if (part == "hair")
        {
            part_ = DataBase.Instance.itemInfoDict[defaultHair[currentHairIdx].itemName].itemType;
            path = DataBase.Instance.itemInfoDict[defaultHair[currentHairIdx].itemName].spriteDirectory;
        }
        else if (part == "cloth")
        {
            part_ = DataBase.Instance.itemInfoDict[defaultCloth[currentClothIdx].itemName].itemType;
            path = DataBase.Instance.itemInfoDict[defaultCloth[currentClothIdx].itemName].spriteDirectory;
        }
        else if (part == "roll")
        {
            part_ = DataBase.Instance.itemInfoDict[defaultWeapon[currentRollIdx].itemName].itemType;
            path = DataBase.Instance.itemInfoDict[defaultWeapon[currentRollIdx].itemName].spriteDirectory;

            CreateCharacterSample.changeSprite("weapon left", null);
            CreateCharacterSample.changeSprite("weapon right", null);
        }
        CreateCharacterSample.changeSprite(part_, path);        
    }


    public void ClickCreateButton()
    {
        if (CheckDuplicateNickName(CreatCharacterNickInput.text))
        {
            StartCoroutine(popMessage("오류", "중복된 닉네임 입니다."));
            return;
        }
            

        CharacterSpec spec = new CharacterSpec();
        CharacterSpec defaultSpec = defaultCharacterSpec[currentRollIdx];
        List<InventoryItem> equipment = new List<InventoryItem>();
        List<Color> colors = new List<Color>();

        equipment.Add(defaultWeapon[currentRollIdx]);
        equipment.Add(defaultCloth[currentClothIdx]);
        equipment.Add(defaultHair[currentHairIdx]);

        colors.Add(currentHairColor);
        colors.Add(currentEyeColor);
        colors.Add(currentEyeColor);

        spec.nickName = CreatCharacterNickInput.text;
        spec.roll = rollList[currentRollIdx];
        spec.characterLevel = 1;
        spec.lastTown = "Pallet Town";

        spec.maxHealth = defaultSpec.maxHealth;
        spec.maxMana = defaultSpec.maxMana;
        spec.recoverManaPerThreeSec = defaultSpec.recoverManaPerThreeSec;
        spec.power = defaultSpec.power;
        spec.criticalDamage = defaultSpec.criticalDamage;
        spec.criticalPercent = defaultSpec.criticalPercent;
        spec.healPercent = defaultSpec.healPercent;

        spec.maxInventoryNum = defaultSpec.maxInventoryNum;
        spec.skillLevel = defaultSpec.skillLevel;
        spec.inventory = defaultSpec.inventory;
        spec.equipment = equipment;
        spec.colors = colors;

        CreateNewCharacter(spec);
    }


    public void ClickNextButton(string part)
    {
        if (part == "roll")
        {
            currentRollIdx++;
            if (currentRollIdx == rollList.Count)
                currentRollIdx = 0;
            CreateCharacterRollText.text = rollList[currentRollIdx];
        }
        else if (part == "hair")
        {
            currentHairIdx++;
            if (currentHairIdx == defaultHair.Count)
                currentHairIdx = 0;
        }
        else if (part == "cloth")
        {
            currentClothIdx++;
            if (currentClothIdx == defaultCloth.Count)
                currentClothIdx = 0;
        }
        UpdateSample(part);
    }

    public void ClickPrevButton(string part)
    {
        if (part == "roll")
        {
            currentRollIdx--;
            if (currentRollIdx == -1)
                currentRollIdx = rollList.Count - 1;
            CreateCharacterRollText.text = rollList[currentRollIdx];
        }
        else if (part == "hair")
        {
            currentHairIdx--;
            if (currentHairIdx == -1)
                currentHairIdx = defaultHair.Count - 1;
        }
        else if (part == "cloth")
        {
            currentClothIdx--;
            if (currentClothIdx == -1)
                currentClothIdx = defaultCloth.Count - 1;
        }
        UpdateSample(part);
    }

    public void ClickColor(string part)
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        Color color = current_clicked_button.GetComponent<Image>().color;
        if (part == "hair")
        {
            CreateCharacterSample.setColors(0, color.r, color.g, color.b);
            currentHairColor = color;
        }
        else if (part == "eye")
        {
            CreateCharacterSample.setColors(1, color.r, color.g, color.b);
            CreateCharacterSample.setColors(2, color.r, color.g, color.b);
            currentEyeColor = color;
        }
    }
    public void ClickCancleCreateButton()
    {
        CharacterCreatePanel.SetActive(false);
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
