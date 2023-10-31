using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;
using Unity.VisualScripting;

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
    public Button RegisterButton;

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
    public bool useLocal;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        Application.targetFrameRate = 60;
        PhotonNetwork.AutomaticallySyncScene = true;
        if(useLocal)
        {
            RegisterButton.interactable = false;
        }
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
                    StartCoroutine(popMessage("가입 완료", "로그인해주세요."));
                }
                else
                {
                    Debug.Log("fail");
                    StartCoroutine(popMessage("가입 실패", "중복된 아이디 입니다."));
                }
            }
            else
            {
                Debug.Log("wrong");
                StartCoroutine(popMessage("오류", "비밀번호가 일치하지 않습니다."));
            }
        }
    }

    

    public void CreateNewCharacter(CharacterSpec newSpec)
    {
        string ID = DataBase.Instance.defaultAccountInfo.accountId;
        string newNickName = newSpec.nickName;


    }

    void ClearSample()
    {
        CreatCharacterNickInput.text = "";
        currentClothIdx = 0;
        currentHairIdx = 0;
        currentRollIdx = 0;
        UpdateSample("cloth");
        UpdateSample("hair");
        UpdateSample("roll");
        CharacterCreatePanel.SetActive(false);
    }

    public void CreateNewCharacterInLocal(CharacterSpec newSpec)
    {
        DataBase.Instance.defaultAccountInfo.characterList.Add(newSpec);
        updateCharacterList();
        ClearSample();
    }


    public void ClickLoginButton()
    {
        if (useLocal)
        {
            ConnectWithOutLogin();
        }
        else
        {
            string loginId = idInputField.text;
            string loginPw = pwInputField.text;

            if (!pwCheckInputField.gameObject.activeSelf)
            {
                int loginStatus = AccountDB.Login(loginId, loginPw);

                if (loginStatus == 1)
                {
                    Debug.Log("Success");
                    DataBase.Instance.defaultAccountInfo.accountId = loginId;
                    DataBase.Instance.defaultAccountInfo.characterList = AccountDB.SelectCharacter(loginId);
                    LoginButton.enabled = false;
                    PopPanel.SetActive(true);
                    StartCoroutine(LoginMessageUpdate());
                    PhotonNetwork.ConnectUsingSettings();
                }
                else if (loginStatus == 0) 
                {
                    Debug.Log("Fail");
                    StartCoroutine(popMessage("로그인 실패", "아이디 혹은 비밀번호가 일치하지 않습니다."));
                }
                else
                {
                    StartCoroutine(popMessage("로그인 실패", "서버에 문제가 있습니다."));
                }
            }
        }
    }

    public void ConnectWithOutLogin()
    {
        LoginButton.interactable = false;
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
        LoginButton.interactable = true;
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
        foreach (Transform character in characterSelectList.transform)
        {
            Destroy(character.gameObject);
        }

        for (int k = 0; k < DataBase.Instance.defaultAccountInfo.characterList.Count; k++)
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
        if (!useLocal)
        {
            if (AccountDB.CheckDuplicateNickName(CreatCharacterNickInput.text))
            {
                StartCoroutine(popMessage("중복", "중복된 닉네임 입니다."));
            }
            else
            {
                StartCoroutine(popMessage("사용 가능", "사용 가능한 닉네임 입니다."));
            }
        }
        else
        {
            StartCoroutine(popMessage("사용 가능", "사용 가능한 닉네임 입니다."));
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

        if (CreatCharacterNickInput.text.IsNullOrEmpty())
        {
            StartCoroutine(popMessage("오류", "닉네임을 입력해주세요."));
            return;
        }
        if (!useLocal)
        {
            if (AccountDB.CheckDuplicateNickName(CreatCharacterNickInput.text))
            {
                StartCoroutine(popMessage("중복", "중복된 닉네임 입니다."));
                return;
            }
        }

        CharacterDB.CreateCharacter(CreatCharacterNickInput.text, "warrior");

        CharacterSpec spec = ScriptableObject.CreateInstance<CharacterSpec>();
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

        if (useLocal)
        {
            CreateNewCharacterInLocal(spec);
        }
        else
        {
            CreateNewCharacter(spec);
        }
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
        ClearSample();
        //CharacterCreatePanel.SetActive(false);
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
        popTitle.text = "로그인 중";
        while (true)
        {
            popContent.text = PhotonNetwork.NetworkClientState.ToString();
            if (PhotonNetwork.IsConnected)
                break;
            yield return null;
        }
    }

}
