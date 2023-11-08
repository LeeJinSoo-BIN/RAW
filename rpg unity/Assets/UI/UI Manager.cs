using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using WebSocketSharp;
using System.Linq;
using System;
using static UnityEditor.Progress;
using static UnityEditor.PlayerSettings;

public class UIManager : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{
    [Header("Panel")]
    #region
    public GameObject currentFocusWindow;
    
    public GameObject enterDungeonPanel;
    public GameObject gameOverPanel;
    public TMP_InputField timeLimitInputfield;
    public GameObject toolTipPanel;
    public GameObject conversationPanel;

    [Header("Inveontory Panel")]
    public GameObject inventoryPanel;
    public GameObject inventoryBox;

    [Header("Option Panel")]
    public GameObject optionPanel;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Text windowText;
    public TMP_Text exitButtonText;

    [Header("Skill Panel")]
    public GameObject skillPanel;
    public GameObject skillBox;
    public GameObject skillInfo;

    [Header("Party Panel")]
    public GameObject partyPanel;
    public GameObject inGameUserPanel;
    public GameObject inGameUserBox;
    public GameObject inGameUserInfo;
    public GameObject partyMemberBox;
    public GameObject partyMemberInfo;
    public TMP_InputField partyMakeNameInput;

    public GameObject partyListPanel;
    public GameObject partyListBox;
    public GameObject partyListInfo;

    public GameObject invitePartyPanel;
    public GameObject joinPartyRequestPanel;

    [Header("Store Panel")]
    public GameObject storePanel;
    public GameObject storeBox;
    public GameObject storeInvenBox;
    public GameObject storeItemInfo;
    public GameObject storeInvenItemInfo;
    public GameObject storeBuyPanel;
    public GameObject storeSellPanel;

    [Header("Enchant Panel")]
    public GameObject EnchantPanel;
    public GameObject EnchantResult;
    #endregion


    [Header("UI")]
    #region
    public Slider myCharacterHealthUi;
    private Slider characterHealth;
    public TMP_Text myCharacterCurrentHealthText;
    public TMP_Text myCharacterMaxHealthText;


    public Slider myCharacterManaUi;
    private Slider characterMana;
    public TMP_Text myCharacterCurrentManaText;
    public TMP_Text myCharacterMaxManaText;


    public GameObject BossUiGroup;
    public Slider bossHealthUi;
    private Slider bossHealth;
    public TMP_Text bossCurrentHealthText;
    public TMP_Text bossMaxHealthText;

    public GameObject quiclSlotUI;
    public GameObject myCharacterProfileUiGroup;

    public GameObject StageUiGroup;
    public TMP_Text stageText;
    public TMP_Text timerText;

    

    [Header("Chat")]
    public TMP_InputField chatInput;
    public TMP_Text chatLogShow;
    public RectTransform ChatBox;
    public RectTransform ChatExpandButtonIcon;
    public Button ChatExpandButton;
    #endregion

    [Header("Daata")]
    #region
    public static UIManager Instance;
    private EventSystem eventSystem;
    public GameObject PlayerGroup;
    public GameObject EnemyGroup;
    public newNetworkManager networkManager;
    public GameObject Boss;
    private MonsterState bossState;
    private bool isBossConnected;
    public float limitTime;
    public float stageTime;
    public IEnumerator timer;


    public Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    private List<string> quickSlotKeys = new List<string> { "1", "2", "3", "4" };
    public Dictionary<string, string> keyToItemName = new Dictionary<string, string>();
    public Dictionary<string, QuickInventory> quickInventory = new Dictionary<string, QuickInventory>();
    public Dictionary<string, CharacterState> inGameUserList = new Dictionary<string, CharacterState>();
    public Dictionary<int, string> idToNickName = new Dictionary<int, string>();
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();
    private Color failColor = new Color((94f / 255f), 0, 0);
    private Color succesColor = new Color(0, (94f / 255f), 0);
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    private bool chatEnd = false;
    public string chatLog;

    public float stayTime = 1f;
    public bool isHoverToolTip = false;
    private GameObject hoverObject;
    private float hoverTime = 0f;
    private float storeBuyDoubleClickTimer = 0f;
    private float storeSellDoubleClickTimer = 0f;
    

    public Dictionary<string, party> allPartys = new Dictionary<string, party>();
    public struct party
    {
        public string captainName;
        public string partyName;
        public HashSet<string> partyMembersNickName;
    }
    #endregion

    void Awake()
    {
        var obj = FindObjectsOfType<UIManager>();
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (Instance == null)
            Instance = this;        

        inventoryPanel.SetActive(false);
        enterDungeonPanel.SetActive(false);
        optionPanel.SetActive(false);
        skillPanel.SetActive(false);
        partyPanel.SetActive(false);
        storePanel.SetActive(false);
        storeBuyPanel.SetActive(false);
        storeSellPanel.SetActive(false);
        BossUiGroup.SetActive(false);
        StageUiGroup.SetActive(false);
        gameOverPanel.SetActive(false);
        toolTipPanel.SetActive(false);
        conversationPanel.SetActive(false);

        invitePartyPanel.SetActive(false);
        joinPartyRequestPanel.SetActive(false);
        inGameUserInfo.SetActive(false);
        partyMemberInfo.SetActive(false);
        partyListInfo.SetActive(false);

        skillInfo.SetActive(false);

        storeInvenItemInfo.SetActive(false);
        storeItemInfo.SetActive(false);
        

        chatInput.onSubmit.AddListener(delegate { sendChat(); });
    }
    // Update is called once per frame
    void Update()
    {
        if (draging)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentFocusWindow.transform.position = new Vector3(currentMousePos.x + distanceMosePos.x, currentMousePos.y + distanceMosePos.y);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentFocusWindow != null)
            {
                currentFocusWindow.SetActive(false);
                openedWindows.Remove(currentFocusWindow);
                updateCurrentFocusWindow();
            }
            else
            {
                updateCurrentFocusWindow(optionPanel);
            }
        }
        if (!chatInput.isFocused && !partyMakeNameInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.P))
            {
                GameObject currentKeyDownPanel = null;
                if (Input.GetKeyDown(KeyCode.I))
                {
                    currentKeyDownPanel = inventoryPanel;
                }
                else if (Input.GetKeyDown(KeyCode.K))
                {
                    currentKeyDownPanel = skillPanel;
                }
                else if (Input.GetKeyDown(KeyCode.P))
                {
                    if (DataBase.Instance.currentMapType == "dungeon")
                        return;
                    currentKeyDownPanel = partyPanel;
                }
                if (currentKeyDownPanel == null)
                    return;

                if (currentKeyDownPanel.activeSelf)
                {
                    if (currentFocusWindow == currentKeyDownPanel)
                    {
                        currentKeyDownPanel.SetActive(false);
                        openedWindows.Remove(currentKeyDownPanel);
                        updateCurrentFocusWindow();
                    }
                    else
                    {
                        if (currentFocusWindow != null)
                            currentFocusWindow.GetComponent<Canvas>().sortingOrder -= 1;
                        currentKeyDownPanel.GetComponent<Canvas>().sortingOrder = openedWindows.Count + 5;
                        currentFocusWindow = currentKeyDownPanel;
                    }
                }
                else
                {
                    updateCurrentFocusWindow(currentKeyDownPanel);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (DataBase.Instance.currentMapType == "village")
                    return;
                string now_input_key = Input.inputString;
                useQuickSlot(now_input_key);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                currentFocusWindow = null;                
            }
        }
        if (isHoverToolTip)
        {
            hoverTime += Time.deltaTime;
            if(hoverTime > stayTime)
            {
                ShowToolTip();
            }
        }
        if (PhotonNetwork.InRoom)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!chatInput.isFocused && !chatEnd)
                {
                    chatInput.ActivateInputField();
                }
                chatEnd = false;
            }
        }
    }


    #region 세팅
    public void SetUP()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<newNetworkManager>();
        PlayerGroup = GameObject.Find("Player Group");
        EnemyGroup = GameObject.Find("Enemy Group");
        GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GetComponent<Canvas>().sortingLayerName = "ui";
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        ResetSkillPanel();
        UpdateSkillPanel();


        UpdatePartyPanel();

        makeProfile();
        characterHealth = DataBase.Instance.myCharacterState.health;
        characterMana = DataBase.Instance.myCharacterState.mana;
        keyToItemName.Clear();
        timerText.text = "00:00:00";
        for (int k = 0; k < quickSlotKeys.Count; k++)
        {
            keyToItemName.Add(quickSlotKeys[k], "");
        }
        setKeyMap();
        StartCoroutine(update_health());
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            StageUiGroup.SetActive(true);
        }
        else
        {
            StageUiGroup.SetActive(false);
        }
        BossUiGroup.SetActive(false);
    }
    public void BossSetUp()
    {
        foreach (Transform monster in EnemyGroup.transform)
        {
            if (monster.GetComponent<MonsterControl>().monsterSpec.monsterType.ToLower() == "boss")
            {
                Boss = monster.gameObject;
                break;
            }
        }
        bossState = Boss.GetComponentInChildren<MonsterState>();
        bossHealth = bossState.health;
        bossHealthUi.maxValue = bossHealth.maxValue;
        isBossConnected = true;
        BossUiGroup.SetActive(true);
    }
    IEnumerator update_health()
    {
        while (true)
        {
            myCharacterHealthUi.value = characterHealth.value;
            myCharacterHealthUi.maxValue = characterHealth.maxValue;
            myCharacterCurrentHealthText.text = ((int)myCharacterHealthUi.value).ToString();
            myCharacterMaxHealthText.text = ((int)myCharacterHealthUi.maxValue).ToString();

            myCharacterManaUi.value = characterMana.value;
            myCharacterManaUi.maxValue = characterMana.maxValue;
            myCharacterCurrentManaText.text = ((int)myCharacterManaUi.value).ToString();
            myCharacterMaxManaText.text = ((int)myCharacterManaUi.maxValue).ToString();

            if (isBossConnected)
            {
                bossHealthUi.value = bossHealth.value;
                bossHealthUi.maxValue = bossHealth.maxValue;
                bossMaxHealthText.text = ((int)bossHealthUi.maxValue).ToString();
                bossCurrentHealthText.text = ((int)bossHealthUi.value).ToString();
            }
            yield return null;
        }
    }

    void makeNewHead(GameObject head)
    {
        SpriteRenderer spriteRenderer = head.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            head.transform.AddComponent<Image>();
            Image imageComponent = head.GetComponent<Image>();
            imageComponent.color = spriteRenderer.color;
            imageComponent.sprite = spriteRenderer.sprite;
            if (imageComponent.sprite == null)
                imageComponent.gameObject.SetActive(false);
            Destroy(spriteRenderer);
        }
        for (int k = 0; k < head.transform.childCount; k++)
        {
            makeNewHead(head.transform.GetChild(k).gameObject);
        }
    }
    public void makeProfile()
    {
        GameObject myCharacterHead = Instantiate(DataBase.Instance.myCharacter.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
        makeNewHead(myCharacterHead);

        foreach (Transform child in myCharacterProfileUiGroup.transform.GetChild(0))
            Destroy(child.gameObject);
        myCharacterHead.transform.parent = myCharacterProfileUiGroup.transform.GetChild(0).transform;
        myCharacterHead.transform.localPosition = new Vector3(0f, -30f, 0f);
        myCharacterProfileUiGroup.transform.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + DataBase.Instance.myCharacterState.characterSpec.characterLevel.ToString();
    }

    #endregion


    #region 채팅
    public void sendChat()
    {
        if (chatInput.text != "")
        {
            string chat = chatInput.text;
            networkManager.PV.RPC("sendChatLog", RpcTarget.All, PhotonNetwork.NickName + " : " + chat);
            chatInput.text = "";
            chatEnd = false;
        }
        else
        {
            chatInput.DeactivateInputField();
            chatEnd = true;
        }
    }

    public void updateChatLog()
    {
        chatLogShow.text = chatLog;
    }

    public void ClickExpandChatLog()
    {
        if (ChatBox.sizeDelta.y == 120)
            ChatBox.sizeDelta = new Vector2(ChatBox.sizeDelta.x, 500);
        else
            ChatBox.sizeDelta = new Vector2(ChatBox.sizeDelta.x, 120);
        ChatExpandButtonIcon.localScale = new Vector3(ChatExpandButtonIcon.localScale.x, -ChatExpandButtonIcon.localScale.y, 1);
    }

    #endregion


    #region 퀵슬릇, 인벤토리
    public void CoolDown(string skillName, float coolingTime)
    {
        StartCoroutine(CoolDownCoroutine(skillName, coolingTime));
    }
    IEnumerator CoolDownCoroutine(string skill_name, float coolingTime)
    {
        string key = skillNameToKey[skill_name];
        Transform currentKeyUI = quiclSlotUI.transform.Find(key.ToLower());
        if (currentKeyUI == null) yield break;
        Image skill_cool = currentKeyUI.GetChild(1).GetComponent<Image>();
        skill_cool.fillAmount = 100;
        float _time = coolingTime;
        if (coolingTime == 0)
            skill_cool.fillAmount = 0;
        while (_time >= 0 && coolingTime > 0)
        {
            _time -= Time.deltaTime;
            skill_cool.fillAmount = _time / coolingTime;
            yield return null;
        }

    }
    public void setKeyMap()
    {
        List<string> keys = skillNameToKey.Values.ToList();
        List<string> skillNames = skillNameToKey.Keys.ToList();
        for (int k = 0; k < skillNameToKey.Count; k++)
        {
            string key = keys[k].ToLower();
            if (key == "q" || key == "w" || key == "e" || key == "r")
            {
                Transform currentSlot = quiclSlotUI.transform.Find(key);
                currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(DataBase.Instance.skillThumbnailPath, skillNames[k]));
                currentSlot.GetChild(0).name = "skill " + skillNames[k];
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = keys[k];
                currentSlot.GetChild(3).GetComponent<TMP_Text>().text = DataBase.Instance.skillInfoDict[skillNames[k]].consumeMana.ToString();
                StartCoroutine(CoolDownCoroutine(skillNames[k], 0f));
            }
        }
        setQuickSlot("1", "red potion small");
        setQuickSlot("2", "blue potion small");
    }

    void useQuickSlot(string key)
    {
        if (!quickInventory.ContainsKey(key))
            return;
        int invenPos = quickInventory[keyToItemName[key]].position.Max;
        if (quickInventory.ContainsKey(keyToItemName[key]))
        {
            if (quickInventory[keyToItemName[key]].kindCount > 0)
            {
                DataBase.Instance.myCharacterControl.loseItem(keyToItemName[key], 1);
                consumePotion(keyToItemName[key]);
            }
            updateInventory();
            updateThisQuickSlot(key);
        }
    }
    public void updateAllQuickSlot(bool updateSprite = false)
    {
        for (int k = 0; k < quickSlotKeys.Count; k++)
        {
            Transform currentSlot = quiclSlotUI.transform.Find(quickSlotKeys[k].ToLower());
            if (updateSprite)
            {
                currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[keyToItemName[quickSlotKeys[k]]].spriteDirectory);
                currentSlot.GetChild(0).name = "item " + keyToItemName[quickSlotKeys[k]];
            }

            if (quickInventory.ContainsKey(keyToItemName[quickSlotKeys[k]]))
            {
                currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
                currentSlot.GetChild(0).name = "item " + keyToItemName[quickSlotKeys[k]];
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[keyToItemName[quickSlotKeys[k]]].kindCount.ToString();
            }
            else
            {
                currentSlot.GetChild(0).GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);                
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
            }
        }
    }
    void updateThisQuickSlot(string key, bool updateSprtie = false)
    {
        Transform currentSlot = quiclSlotUI.transform.Find(key);
        if (updateSprtie)
        {
            currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[keyToItemName[key]].spriteDirectory);
            currentSlot.GetChild(0).name = "item " + keyToItemName[key];
        }
        if (quickInventory.ContainsKey(keyToItemName[key]))
        {
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
            currentSlot.GetChild(0).name = "item " + keyToItemName[key];
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[keyToItemName[key]].kindCount.ToString();
        }
        else
        {
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.gray;            
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
        }
    }

    void setQuickSlot(string key, string itemName)
    {
        keyToItemName[key] = itemName;
        updateThisQuickSlot(key, true);
    }


    void consumePotion(string itemName)
    {
        DataBase.Instance.myCharacterState.ProcessSkill(1, DataBase.Instance.itemInfoDict[itemName].recoveryHealth);
        DataBase.Instance.myCharacterState.ProcessSkill(5, DataBase.Instance.itemInfoDict[itemName].recoveryMana);
    }

    public void updateInventory()
    {
        inventoryPanel.transform.GetChild(4).GetComponent<TMP_Text>().text = DataBase.Instance.myCharacterState.characterSpec.money.ToString();
        for (int pos = 0; pos < DataBase.Instance.myCharacterState.characterSpec.maxInventoryNum; pos++)
        {
            Transform box = inventoryBox.transform.GetChild(pos);
            InventoryItem item = DataBase.Instance.myCharacterState.characterSpec.inventory[pos];
            if (item == null)
            {
                box.GetChild(2).GetComponent<TMP_Text>().text = "";
                box.GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
                box.GetChild(1).name = "";
            }
            else
            {
                box.GetChild(2).GetComponent<TMP_Text>().text = item.count.ToString();
                box.GetChild(1).GetComponent<Image>().color = Color.white;
                box.GetChild(1).name = "item " + item;
                if (box.GetChild(1).GetComponent<Image>().color.a == 0)
                    box.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory);
            }
        }
    }
    #endregion


    #region 패널
    public void updateCurrentFocusWindow(GameObject currentWindow = null)
    {
        if (currentWindow != null)
        {
            currentFocusWindow = currentWindow;
            openedWindows.Add(currentWindow);
            currentWindow.GetComponent<Canvas>().sortingOrder = openedWindows.Count + 5;
            currentFocusWindow.SetActive(true);
        }
        else
        {
            if(currentFocusWindow == storePanel)
            {
                if (storeBuyPanel.activeSelf)
                {
                    storeBuyPanel.SetActive(false);
                    openedWindows.Remove(storeBuyPanel);
                }
                if(storeSellPanel.activeSelf)
                {
                    storeSellPanel.SetActive(false);
                    openedWindows.Remove(storeSellPanel);
                }
            }
            if (openedWindows.Count > 0)
            {
                foreach (GameObject window in openedWindows)
                {
                    if (window.GetComponent<Canvas>().sortingOrder == openedWindows.Count + 5)
                        currentFocusWindow = window;
                }
            }
            else
                currentFocusWindow = null;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedPanel = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        if (clickedObject.name == "back" || clickedObject.name == "drag area")
        {
            updateCurrentFocusWindow();
            currentFocusWindow.GetComponent<Canvas>().sortingOrder -= 1;
            clickedPanel.GetComponent<Canvas>().sortingOrder = openedWindows.Count + 5;
            currentFocusWindow = clickedPanel;
            if (clickedObject.name == "drag area")
            {
                Vector3 dragStartMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                distanceMosePos.x = clickedPanel.transform.position.x - dragStartMousePos.x;
                distanceMosePos.y = clickedPanel.transform.position.y - dragStartMousePos.y;
                draging = true;
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        draging = false;
    }
    #endregion


    #region 툴팁
    public void EnterToolTip()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);        
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);        
        if(results.Count > 0)
        {            
            isHoverToolTip = true;
            hoverObject = results[0].gameObject;
        }
    }

    public void ExitToolTip()
    {
        isHoverToolTip = false;
        hoverObject = null;
        hoverTime = 0f;
        toolTipPanel.SetActive(false);
    }

    void ShowToolTip()
    {
        if (!hoverObject.activeInHierarchy)
        {
            hoverTime = 0;
            toolTipPanel.SetActive(false);
            return;
        }
        string toolTipName;
        string toolTipContent;
        if (hoverObject.name.Contains("skill"))
        {
            toolTipName = hoverObject.name.Substring(6);
            toolTipContent = DataBase.Instance.skillInfoDict[toolTipName].description;

            toolTipContent = toolTipContent.Replace("(sumDeal)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatDeal +
                DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerSkillLevel * DataBase.Instance.myCharacterState.characterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatDeal)", DataBase.Instance.skillInfoDict[toolTipName].flatDeal.ToString());
            toolTipContent = toolTipContent.Replace("(dealIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(dealIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(sumHeal)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatHeal +
                DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerSkillLevel * DataBase.Instance.myCharacterState.characterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatHeal)", DataBase.Instance.skillInfoDict[toolTipName].flatHeal.ToString());
            toolTipContent = toolTipContent.Replace("(healIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(healIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(sumShield)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatShield +
                DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerSkillLevel * DataBase.Instance.myCharacterState.characterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatShield)", DataBase.Instance.skillInfoDict[toolTipName].flatShield.ToString());
            toolTipContent = toolTipContent.Replace("(shieldIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(shieldIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(sumPower)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatPower +
                DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerSkillLevel * DataBase.Instance.myCharacterState.characterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatPower)", DataBase.Instance.skillInfoDict[toolTipName].flatPower.ToString());
            toolTipContent = toolTipContent.Replace("(powerIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(powerIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(coolDown)", DataBase.Instance.skillInfoDict[toolTipName].coolDown.ToString());
            toolTipContent = toolTipContent.Replace("(consumeMana)", DataBase.Instance.skillInfoDict[toolTipName].consumeMana.ToString());
            toolTipContent = toolTipContent.Replace("(duration)", DataBase.Instance.skillInfoDict[toolTipName].duration.ToString());
        }
        else if (hoverObject.name.Contains("item"))
        {
            toolTipName = hoverObject.name.Substring(5);
            toolTipContent = DataBase.Instance.itemInfoDict[toolTipName].description;

            toolTipContent = toolTipContent.Replace("(health)", DataBase.Instance.itemInfoDict[toolTipName].recoveryHealth.ToString());
            toolTipContent = toolTipContent.Replace("(mana)", DataBase.Instance.itemInfoDict[toolTipName].recoveryMana.ToString());

        }
        else
            return;
        
        toolTipPanel.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = toolTipName;
        toolTipPanel.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = toolTipContent;        
        toolTipPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 70 + toolTipPanel.transform.GetChild(1).GetChild(1).GetComponent<RectTransform>().sizeDelta.y);
        toolTipPanel.transform.position = hoverObject.transform.position;        
        toolTipPanel.SetActive(true);
    }
    #endregion


    #region 스킬
    public void ResetSkillPanel()
    {
        for (int k = 0; k < skillBox.transform.childCount; k++)
        {
            Destroy(skillBox.transform.GetChild(k).gameObject);
        }
    }
    public void UpdateSkillPanel()
    {
        List<string> skillName = DataBase.Instance.myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel.SD_Keys;
        foreach (string name in skillName)
        {
            if (name.Contains("normal"))
                continue;
            if (skillBox.transform.Find(name) == null)
            {
                GameObject newSkill = Instantiate(skillInfo);
                newSkill.name = "skill " + name;
                newSkill.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(DataBase.Instance.skillThumbnailPath, name));
                newSkill.transform.GetChild(1).name = "skill " + name;
                newSkill.transform.GetChild(2).GetComponent<TMP_Text>().text = name;
                string max_level = DataBase.Instance.skillInfoDict[name].maxLevel.ToString();
                string current_level = DataBase.Instance.myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel[name].ToString();
                newSkill.transform.GetChild(3).GetComponent<TMP_Text>().text = current_level + " / " + max_level;
                newSkill.transform.SetParent(skillBox.transform, false);
                newSkill.transform.localPosition = Vector3.zero;
                newSkill.transform.localScale = Vector3.one;
                newSkill.gameObject.SetActive(true);
            }
            else
            {
                string max_level = DataBase.Instance.skillInfoDict[name].maxLevel.ToString();
                string current_level = DataBase.Instance.myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel[name].ToString();
                skillBox.transform.Find(name).transform.GetChild(3).GetComponent<TMP_Text>().text = current_level + " / " + max_level;
            }
        }
    }
    public void ClickSkillLevelUpButton()
    {

    }

    #endregion


    #region 던전
    public IEnumerator startTimer()
    {
        stageTime = 0f;
        while (true)
        {
            stageTime += Time.deltaTime;
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", (int)stageTime / 3600, (int)stageTime / 60 % 60, (int)stageTime % 60);
            if (stageTime >= limitTime)
                break;
            yield return null;
        }
        EndGame("time out");


    }
    public void EndGame(string condition)
    {
        string title = null;
        string content = null;
        StopCoroutine(timer);
        if (condition == "time out")
        {
            title = "타임아웃";
            gameOverPanel.transform.GetChild(0).GetComponent<Image>().color = failColor;
            content = string.Format("남은 체력\n{0}\n\n소요시간\n{1}초\n\n인 원\n", bossCurrentHealthText.text, limitTime.ToString());
        }
        else if (condition == "clear")
        {
            title = "클리어";
            gameOverPanel.transform.GetChild(0).GetComponent<Image>().color = succesColor;
            content = string.Format("클리어 시간\n{0}초\n\n인 원\n", stageTime.ToString());

        }
        else if (condition == "all death")
        {
            title = "실패";
            gameOverPanel.transform.GetChild(0).GetComponent<Image>().color = failColor;
            content = string.Format("남은 체력\n{0}\n\n소요시간\n{1}초\n\n인 원\n", bossCurrentHealthText.text, stageTime.ToString());

        }
        foreach (Transform player in PlayerGroup.transform)
        {
            content += player.GetComponent<CharacterState>().nick + "(" + player.GetComponent<CharacterState>().roll + ") ";
            player.GetComponent<MultyPlayer>().isDeath = true;
        }
        foreach (Transform monster in EnemyGroup.transform)
        {
            monster.GetComponent<MonsterControl>().attackable = false;
        }

        gameOverPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = title;
        gameOverPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = content;


        if (DataBase.Instance.isCurrentDungeonCaptain)
        {
            gameOverPanel.transform.GetChild(3).GetComponent<Button>().interactable = true;
            gameOverPanel.transform.GetChild(4).GetComponent<Button>().interactable = true;
        }
        else
        {
            gameOverPanel.transform.GetChild(3).GetComponent<Button>().interactable = false;
            gameOverPanel.transform.GetChild(4).GetComponent<Button>().interactable = false;
        }
        gameOverPanel.SetActive(true);
    }

    public void ClickReGameButton()
    {
        networkManager.PV.RPC("ReGame", RpcTarget.All);
    }
    public void ClickGoToVillageButton()
    {
        networkManager.PV.RPC("GoToVillage", RpcTarget.All);
    }

    public void EnterDungeonPop()
    {
        updateCurrentFocusWindow(enterDungeonPanel);
    }

    public void ClickEnterDungeonButton()
    {

        if (!timeLimitInputfield.text.IsNullOrEmpty() && !int.TryParse(timeLimitInputfield.text, out _))
            return;
        float _timeLimit;
        if (timeLimitInputfield.text.IsNullOrEmpty())
            _timeLimit = 0;
        else
            _timeLimit = float.Parse(timeLimitInputfield.text);
        networkManager.movePortal(_timeLimit);
        enterDungeonPanel.SetActive(false);
    }
    #endregion


    #region 파티
    public void UpdatePartyPanel()
    {
        if (!PhotonNetwork.InRoom)
            return;        
        UpdateInGameUser();
        UpdatePartyMember();        
        UpdatePartyList();
    }   
    
    public void UpdateInGameUser()
    {        
        for (int k = 0; k < inGameUserBox.transform.childCount; k++)
        {
            Destroy(inGameUserBox.transform.GetChild(k).gameObject);
        }
        allPartys.Clear();
        inGameUserList.Clear();
        foreach (Transform user in PlayerGroup.transform)
        {
            CharacterState currentUserState = user.GetComponent<CharacterState>();
            inGameUserList.Add(user.name, currentUserState);
            if (!currentUserState.partyCaptainName.IsNullOrEmpty())
            {
                if (allPartys.ContainsKey(currentUserState.partyCaptainName))
                {
                    allPartys[currentUserState.partyCaptainName].partyMembersNickName.Add(user.name);
                }
                else
                {
                    party newParty;
                    newParty.captainName = currentUserState.partyCaptainName;
                    newParty.partyName = currentUserState.partyName;
                    newParty.partyMembersNickName = new HashSet<string> { user.name };
                    allPartys.Add(currentUserState.partyCaptainName, newParty);
                }
            }
        }
        foreach (Transform user in PlayerGroup.transform)
        {
            CharacterState currentUserState = inGameUserList[user.name];
            if (user.GetComponent<PhotonView>().IsMine || !currentUserState.partyCaptainName.IsNullOrEmpty())
                continue;
            GameObject userInfo = Instantiate(inGameUserInfo);
            userInfo.transform.GetChild(0).GetComponent<TMP_Text>().text = "닉네임: " + currentUserState.nick;
            userInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + currentUserState.level.ToString();
            userInfo.transform.GetChild(2).GetComponent<TMP_Text>().text = "직업: " + currentUserState.roll;
            userInfo.transform.GetChild(3).name = user.name;
            if (!DataBase.Instance.isCaptain)
                userInfo.transform.GetChild(3).GetComponent<Button>().interactable = false;
            else if (allPartys[DataBase.Instance.myPartyCaptainName].partyMembersNickName.Count >= 3)
                userInfo.transform.GetChild(3).GetComponent<Button>().interactable = false;

            userInfo.transform.parent = inGameUserBox.transform;
            userInfo.transform.localScale = Vector3.one;
            userInfo.transform.localPosition = Vector3.zero;
            userInfo.SetActive(true);
        }
    }

    public void UpdatePartyMember()
    {
        for (int k = 0; k < partyMemberBox.transform.childCount; k++)
        {
            Destroy(partyMemberBox.transform.GetChild(k).gameObject);
        }

        if (!DataBase.Instance.myPartyCaptainName.IsNullOrEmpty())
        {
            foreach (string memberName in allPartys[DataBase.Instance.myPartyCaptainName].partyMembersNickName)
            {
                CharacterState playerInfo = inGameUserList[memberName];
                if (playerInfo.partyCaptainName == DataBase.Instance.myPartyCaptainName)
                {
                    GameObject newMember = Instantiate(partyMemberInfo);

                    GameObject memberHead = Instantiate(playerInfo.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
                    makeNewHead(memberHead);
                    memberHead.transform.parent = newMember.transform.GetChild(1);
                    memberHead.transform.localPosition = new Vector3(0, -30, 0);
                    memberHead.transform.localScale = new Vector3(120, 120);

                    newMember.transform.GetChild(2).GetComponent<TMP_Text>().text = playerInfo.nick;
                    if (memberName == DataBase.Instance.myPartyCaptainName)
                        newMember.transform.GetChild(2).GetComponent<TMP_Text>().text = "*" + newMember.transform.GetChild(2).GetComponent<TMP_Text>().text;
                    newMember.transform.GetChild(3).GetComponent<TMP_Text>().text = "Lv. " + playerInfo.level.ToString();
                    newMember.transform.GetChild(4).GetComponent<TMP_Text>().text = "직업: " + playerInfo.roll;
                    newMember.transform.GetChild(5).name = memberName;
                    if (!DataBase.Instance.isCaptain)
                        newMember.transform.GetChild(5).GetComponent<Button>().interactable = false;
                    else if (memberName == DataBase.Instance.myCharacter.name)
                        newMember.transform.GetChild(5).GetComponent<Button>().interactable = false;
                    newMember.transform.parent = partyMemberBox.transform;

                    newMember.SetActive(true);
                    newMember.transform.localScale = Vector3.one;
                    newMember.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    void UpdatePartyList()
    {
        for (int k = 0; k < partyListBox.transform.childCount; k++)
        {
            Destroy(partyListBox.transform.GetChild(k).gameObject);
        }
        foreach (party partyInfo in allPartys.Values)
        {

            GameObject newParty = Instantiate(partyListInfo);
            newParty.transform.GetChild(0).GetComponent<TMP_Text>().text = "파티장: " + partyInfo.captainName;
            newParty.transform.GetChild(1).GetComponent<TMP_Text>().text = "파티명: " + partyInfo.partyName;
            newParty.transform.GetChild(2).GetComponent<TMP_Text>().text = "인원: " + partyInfo.partyMembersNickName.Count + "/3";
            newParty.transform.GetChild(3).name = partyInfo.captainName;
            if (!DataBase.Instance.myPartyCaptainName.IsNullOrEmpty() || partyInfo.partyMembersNickName.Count == 3)
            {
                newParty.transform.GetChild(3).GetComponent<Button>().interactable = false;
            }
            newParty.transform.parent = partyListBox.transform;
            newParty.SetActive(true);
            newParty.transform.localScale = Vector3.one;
            newParty.transform.localPosition = Vector3.zero;
        }
    }


    public void ClickMakePartyButton()
    {
        if (!DataBase.Instance.myPartyCaptainName.IsNullOrEmpty())
            return;        
        string partyName = partyMakeNameInput.text;
        if (partyMakeNameInput.text.IsNullOrEmpty())
            partyName = "파티 고고";
        DataBase.Instance.isCaptain = true;
        DataBase.Instance.myPartyCaptainName = DataBase.Instance.myCharacter.name;
        DataBase.Instance.myPartyName = partyName;
        DataBase.Instance.myCharacterState.updateParty();
        networkManager.PV.RPC("UpdateParty", RpcTarget.All);
    }

    public void ClickAcceptPartyInviteButton()
    {
        UpdateInGameUser();
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        if (allPartys[current_clicked_button.name].partyMembersNickName.Count < 3)
        {
            DataBase.Instance.isCaptain = false;
            DataBase.Instance.myPartyCaptainName = current_clicked_button.name;
            DataBase.Instance.myPartyName = allPartys[current_clicked_button.name].partyName;
            DataBase.Instance.myCharacterState.updateParty();
            networkManager.PV.RPC("UpdateParty", RpcTarget.All);
        }
        else
        {

        }
        invitePartyPanel.SetActive(false);
        openedWindows.Remove(invitePartyPanel);
        updateCurrentFocusWindow();
    }

    public void ClickAcceptJoinPartyRequestButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        UpdateInGameUser();
        if (allPartys[DataBase.Instance.myCharacter.name].partyMembersNickName.Count < 3)
        {
            networkManager.PV.RPC("acceptJoinParty", inGameUserList[current_clicked_button.name].PV.Owner, DataBase.Instance.myCharacter.name, DataBase.Instance.myPartyName);
        }
        else
        {

        }
        joinPartyRequestPanel.SetActive(false);
        openedWindows.Remove(joinPartyRequestPanel);
        updateCurrentFocusWindow();
    }


    public void ClickLeavePartyButton()
    {
        if (DataBase.Instance.myPartyCaptainName.IsNullOrEmpty())
            return;
        if (DataBase.Instance.isCaptain)
        {
            if (allPartys[DataBase.Instance.myPartyCaptainName].partyMembersNickName.Count > 1)
            {
                string newCaptainName = "";
                foreach (string memberName in allPartys[DataBase.Instance.myPartyCaptainName].partyMembersNickName)
                {
                    if (memberName == DataBase.Instance.myCharacter.name)
                        continue;
                    if (newCaptainName == "")
                        newCaptainName = memberName;
                    networkManager.PV.RPC("ChangeCaptain", inGameUserList[memberName].PV.Owner, newCaptainName);                    
                }
            }
        }
        DataBase.Instance.isCaptain = false;
        DataBase.Instance.myPartyCaptainName = "";
        DataBase.Instance.myPartyName = "";
        DataBase.Instance.myCharacterState.updateParty();
        networkManager.PV.RPC("UpdateParty", RpcTarget.All);
    }

    public void ClickKickPartyMemberButton()
    {
        if (DataBase.Instance.myPartyCaptainName != DataBase.Instance.myCharacter.name)
            return;
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        networkManager.PV.RPC("kickPartyMember", inGameUserList[current_clicked_button.name].PV.Owner);
    }

    

    public void ClickRejectPartyInviteButton()
    {
        invitePartyPanel.SetActive(false);
        openedWindows.Remove(invitePartyPanel);
        updateCurrentFocusWindow();
    }



    public void ClickRejectJoinPartyRequestButton()
    {
        joinPartyRequestPanel.SetActive(false);
        openedWindows.Remove(joinPartyRequestPanel);
        updateCurrentFocusWindow();
    }


    public void ClickPartyInviteButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        networkManager.PV.RPC("sendAndReceiveInviteParty",
            inGameUserList[current_clicked_button.name].PV.Owner,
            allPartys[DataBase.Instance.myPartyCaptainName].partyName,
            DataBase.Instance.myCharacter.name);
    }

    public void ClickPartyJoinRequsetButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        networkManager.PV.RPC("sendAndReceiveJoinRequestParty",
            inGameUserList[current_clicked_button.name].PV.Owner,
            DataBase.Instance.myCharacter.name);
    }
    public void receiveInvite(string partyName, string captain)
    {        
        updateCurrentFocusWindow(invitePartyPanel);
        CharacterState captainInfo = inGameUserList[captain];
        string captainNick = captainInfo.nick;
        string captainLevel = captainInfo.level.ToString();
        string captainRoll = captainInfo.roll;
        invitePartyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = string.Format("{0}님\r\n레벨: {1}\r\n직업: {2}\r\n이 파티 초대를 보냈습니다.\r\n\r\n파티명: {3}", captainNick, captainLevel, captainRoll, partyName);
        invitePartyPanel.transform.GetChild(2).GetChild(1).name = captain;
    }
    public void receiveJoinRequest(string fromWho)
    {
        updateCurrentFocusWindow(joinPartyRequestPanel);
        CharacterState captainInfo = inGameUserList[fromWho];
        string captainNick = captainInfo.nick;
        string captainLevel = captainInfo.level.ToString();
        string captainRoll = captainInfo.roll;
        joinPartyRequestPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = string.Format("{0}님\r\n레벨: {1}\r\n직업: {2}\r\n이 파티 가입 요청을 보냈습니다.", captainNick, captainLevel, captainRoll);
        joinPartyRequestPanel.transform.GetChild(2).GetChild(1).name = fromWho;
    }
    #endregion

    #region NPC 대화
    public void ShowConversationPanel(GameObject NPC)
    {
        updateCurrentFocusWindow(conversationPanel);
        GameObject npcHead = Instantiate(NPC.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
        makeNewHead(npcHead);

        npcHead.transform.parent = conversationPanel.transform.GetChild(2).GetChild(0);
        npcHead.transform.localPosition = new Vector3(0, -30, 0);
        npcHead.transform.localScale = new Vector3(120, 120);

    }

    #endregion

    #region 상점
    public void ShowStorePanel(GameObject npc)
    {
        UpdateStoreNpc(npc);
        UpdateStoreInventory();
        updateCurrentFocusWindow(storePanel);
    }
    
    public void UpdateStoreNpc(GameObject npc)
    {
        foreach (Transform box in storeBox.transform)
        {
            Destroy(box.gameObject);
        }
        storeBox.transform.parent.GetChild(0).GetComponent<TMP_Text>().text = npc.GetComponent<NPC>().spec.NpcName + "의 상점";
        foreach (InventoryItem item in npc.GetComponent<NPC>().spec.sellingItems)
        {
            GameObject sellingItem = Instantiate(storeItemInfo);
            sellingItem.name = item.itemName;
            sellingItem.transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory);
            sellingItem.transform.GetChild(2).name = "item " + item.itemName;
            sellingItem.transform.GetChild(3).GetComponent<TMP_Text>().text = item.itemName;
            sellingItem.transform.GetChild(4).GetComponent<TMP_Text>().text = DataBase.Instance.itemInfoDict[item.itemName].buyPrice.ToString();
            sellingItem.transform.parent = storeBox.transform;
            sellingItem.transform.localScale = Vector3.one;
            sellingItem.transform.localPosition = Vector3.zero;
            sellingItem.SetActive(true);
        }        
    }
    public void UpdateStoreInventory()
    {
        foreach (Transform item in storeInvenBox.transform)
        {
            Destroy(item.gameObject);
        }
        storeInvenBox.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = DataBase.Instance.myCharacterState.characterSpec.money.ToString();
        foreach (string itemName in quickInventory.Keys)
        {
            GameObject invenItem = Instantiate(storeInvenItemInfo);
            invenItem.name = itemName;
            invenItem.transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].spriteDirectory);
            invenItem.transform.GetChild(2).name = "item " + itemName;
            invenItem.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = quickInventory[itemName].kindCount.ToString();
            invenItem.transform.GetChild(3).GetComponent<TMP_Text>().text = itemName;
            invenItem.transform.GetChild(4).GetComponent<TMP_Text>().text = DataBase.Instance.itemInfoDict[itemName].sellPrice.ToString();
            invenItem.transform.parent = storeInvenBox.transform;
            invenItem.transform.localScale = Vector3.one;
            invenItem.transform.localPosition = Vector3.zero;
            invenItem.SetActive(true);
        }
    }
    public void ClickStoreItem(bool buy)
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string currentItemName = current_clicked_button.name;
        if (buy)
        {            
            if (Time.time - storeBuyDoubleClickTimer < 0.25f)
            {
                updateCurrentFocusWindow(storeBuyPanel);
                storeBuyPanel.transform.GetChild(2).name = currentItemName;
                storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = "1";
                storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().Select();
            }
            else
            {
                Debug.Log(Time.time - storeBuyDoubleClickTimer);
                storeBuyDoubleClickTimer = Time.time;
            }
        }
        else
        {
            if (Time.time - storeSellDoubleClickTimer < 0.25f)
            {
                updateCurrentFocusWindow(storeSellPanel);
                storeSellPanel.transform.GetChild(2).name = currentItemName;
                storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = quickInventory[currentItemName].kindCount.ToString();
                storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().Select();
            }
            else
            {
                Debug.Log(Time.time - storeSellDoubleClickTimer);
                storeSellDoubleClickTimer = Time.time;
            }
        }
    }

    public void ClickSellButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string sellItemName = current_clicked_button.transform.parent.name;
        int sellItemCnt = int.Parse(current_clicked_button.transform.parent.GetChild(0).GetComponent<TMP_InputField>().text);

        if (sellItemCnt > quickInventory[sellItemName].kindCount)
        {
            return;
        }


        int sellMoney = sellItemCnt * DataBase.Instance.itemInfoDict[sellItemName].sellPrice;
        DataBase.Instance.myCharacterState.characterSpec.money += sellMoney;
        DataBase.Instance.myCharacterControl.loseItem(sellItemName, sellItemCnt);        
        updateInventory();
        UpdateStoreInventory();
        storeSellPanel.SetActive(false);
        openedWindows.Remove(storePanel);
        updateCurrentFocusWindow();
    }

    public void ClickBuyButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string buyItemName = current_clicked_button.transform.parent.name;
        int buyItemCnt = int.Parse(current_clicked_button.transform.parent.GetChild(0).GetComponent<TMP_InputField>().text);
        int buyMoney = buyItemCnt * DataBase.Instance.itemInfoDict[buyItemName].buyPrice;
        if(buyMoney > DataBase.Instance.myCharacterState.characterSpec.money)
        {
            return;
        }
        if (DataBase.Instance.myCharacter.GetComponent<MultyPlayer>().getItem(new Item { itemName = buyItemName, itemCount = buyItemCnt }, false))
        {
            DataBase.Instance.myCharacterState.characterSpec.money -= buyMoney;
            UpdateStoreInventory();
            storeBuyPanel.SetActive(false);
            openedWindows.Remove(storeBuyPanel);
            updateCurrentFocusWindow();
        }

    }
    #endregion

    #region 강화
    public void UpdateEnchantPanel()
    {
        string enchantItemName = EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).name;
        //int currentLevel = quickInventory[enchantItemName].position
    }
    public void ClickEnchantButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.parent.GetComponent<Animator>().SetBool("enchant", true);
    }

    public void ClickEnchantResultButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.SetActive(false);
    }
    #endregion

    #region 옵션
    public void setResolution()
    {
        string selected_resolution_string = resolutionDropdown.options[resolutionDropdown.value].text;
        string[] selected_resolution = selected_resolution_string.Split(" x ");
        bool window = false;
        if (windowText.text == "창모드")
            window = true;
        else
            window = false;
        Screen.SetResolution(int.Parse(selected_resolution[0]), int.Parse(selected_resolution[1]), window);
    }

    public void setWindow()
    {
        if (windowText.text == "창모드")
        {
            Screen.fullScreen = false;
            windowText.text = "전체화면";
        }
        else
        {
            Screen.fullScreen = true;
            windowText.text = "창모드";
        }
    }

    public void ClickQuitButton()
    {
        if (Application.isPlaying)
            Application.Quit();
    }
    public void ClickDisconnectButton()
    {
        networkManager.ClickDisconnectButton();
    }
    public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        updateCurrentFocusWindow();
    }
    #endregion




}
