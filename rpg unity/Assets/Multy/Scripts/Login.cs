using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

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
                int registerStatus = AccountDB.Register(idInputField.text, pwInputField.text);

                if (registerStatus == 1)
                {
                    Debug.Log("register success");
                    StartCoroutine(popMessage("???? ???? ????", "?????? ????????."));
                }
                else
                {
                    Debug.Log("fail");
                    StartCoroutine(popMessage("???? ???? ????", "???? ????"));
                }
            }
            else
            {
                Debug.Log("wrong");
                StartCoroutine(popMessage("????", "?????????? ???????? ????????."));
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
                int loginStatus = AccountDB.Login(loginId, loginPw);

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
                    StartCoroutine(popMessage("?????? ????", "???????? ?????????? ????????????."));
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
            StartCoroutine(popMessage("????", "?????? ?????? ??????."));
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
            StartCoroutine(popMessage("????", "?????? ?????? ??????."));
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
        popTitle.text = "??????";
        while (true)
        {
            popContent.text = PhotonNetwork.NetworkClientState.ToString();
            if (PhotonNetwork.IsConnected)
                break;
            yield return null;
        }
    }

}
