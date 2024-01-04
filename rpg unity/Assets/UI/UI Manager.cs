using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using WebSocketSharp;


public class UIManager : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{

    #region Panel
    [Header("Panel")]
    public GameObject currentFocusWindow;
    public GameObject enterDungeonPanel;
    public GameObject gameOverPanel;
    public GameObject toolTipPanel;
    public GameObject conversationPanel;
    public GameObject equipmentPanel;
    public GameObject draggingItem;
    public GameObject loadingPanel;
    public GameObject userInteractionPanel;
    public GameObject infoPopUpPanel;

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

    [Header("Trade Panel")]
    public GameObject tradePanel;
    public GameObject tradeRequestPanel;
    public TMP_InputField tradeChatInput;
    public GameObject myTradeBox;
    public GameObject opTradeBox;
    public TMP_Text tradeChatLogShow;
    public GameObject opAcceptTradeText;
    public GameObject tradeCheckCntPanel;
    #endregion



    #region UI
    [Header("UI")]
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

    public GameObject magnifierIcon;

    [Header("Chat")]
    public TMP_InputField chatInput;
    public TMP_Text chatLogShow;
    public RectTransform ChatBox;
    public RectTransform ChatExpandButtonIcon;
    public Button ChatExpandButton;
    #endregion




    #region Data
    [Header("Daata")]
    public static UIManager Instance;
    private EventSystem eventSystem;
    public GameObject PlayerGroup;
    public GameObject EnemyGroup;
    public GameObject Boss;
    private MonsterState bossState;
    private bool isBossConnected;
    //public float limitTime;
    public float stageTime;
    public IEnumerator timer;


    private Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    private List<string> quickItemSlotKeys = new List<string> { "1", "2", "3", "4" };
    private List<string> quickSkillSlotKeys = new List<string> { "q", "w", "e", "r" };
    public Dictionary<string, QuickInventory> quickInventory = new Dictionary<string, QuickInventory>();
    public Dictionary<string, CharacterState> inGameUserList = new Dictionary<string, CharacterState>();
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();
    private Color failColor = new Color((94f / 255f), 0, 0);
    private Color succesColor = new Color(0, (94f / 255f), 0);
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    private bool chatEnd = false;
    public string chatLog;

    private float stayTime = 1f;
    private bool isHoverToolTip = false;
    private GameObject hoverObject;
    private GameObject dragObject;
    private Vector3 dragObjectOriginPos;
    private float hoverTime = 0f;
    private float storeBuyDoubleClickTimer = 0f;
    private float storeSellDoubleClickTimer = 0f;


    public string tradeChatLog;
    private bool tradeChatEnd = false;
    public LayerMask playerLayer;
    private string tradeOpName;


    public Dictionary<string, party> allPartys = new Dictionary<string, party>();
    public struct party
    {
        public string captainName;
        public string partyName;
        public HashSet<string> partyMembersNickName;
    }


    private int enchantPercent;
    private int[] enchantPrice = new int[2];
    private bool isEnchanting = false;

    private float itemDoubleClickTimer = 0f;
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
        equipmentPanel.SetActive(false);

        loadingPanel.SetActive(false);
        infoPopUpPanel.SetActive(false);

        invitePartyPanel.SetActive(false);
        joinPartyRequestPanel.SetActive(false);
        inGameUserInfo.SetActive(false);
        partyMemberInfo.SetActive(false);
        partyListInfo.SetActive(false);

        skillInfo.SetActive(false);

        storeInvenItemInfo.SetActive(false);
        storeItemInfo.SetActive(false);

        EnchantPanel.SetActive(false);
        EnchantResult.SetActive(false);

        draggingItem.SetActive(false);
        chatInput.onSubmit.AddListener(delegate { sendChat(); });
        tradeChatInput.onSubmit.AddListener(delegate { sendTradeChat(); });
        storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().onSubmit.AddListener(delegate { ClickSellButton(); });
        storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().onSubmit.AddListener(delegate { ClickBuyButton(); });
        tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().onSubmit.AddListener(delegate { ClickTradeCheckCntButton(); });

        tradePanel.SetActive(false);
        tradeRequestPanel.SetActive(false);

        magnifierIcon.SetActive(false);

    }
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (draging)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentFocusWindow.transform.position = new Vector3(currentMousePos.x + distanceMosePos.x, currentMousePos.y + distanceMosePos.y, 0);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentFocusWindow != null)
            {
                if (currentFocusWindow == tradePanel)
                {

                    return;
                }
                currentFocusWindow.SetActive(false);
                openedWindows.Remove(currentFocusWindow);
                updateCurrentFocusWindow();
            }
            else
            {
                updateCurrentFocusWindow(optionPanel);
            }
        }
        if (!chatInput.isFocused && !partyMakeNameInput.isFocused && !tradeChatInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.U))
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
                    currentKeyDownPanel = partyPanel;
                }
                else if (Input.GetKeyDown(KeyCode.U))
                {
                    currentKeyDownPanel = equipmentPanel;
                    if (!currentKeyDownPanel.activeSelf)
                        UpdateEquipmentPanel();
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

            if (magnifierIcon.activeSelf)
            {
                Vector3 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, playerLayer);
                if (hit.collider == null)
                    return;
                if (!inGameUserList.ContainsKey(hit.collider.transform.name))
                    UpdateInGameUser();
                if (hit.collider.gameObject == DataBase.Instance.myCharacter)
                    return;
                ShowUserInteractionPanel(hit.collider.transform.name);
            }
        }
        if (isHoverToolTip)
        {
            hoverTime += Time.deltaTime;
            if (hoverTime > stayTime)
            {
                ShowToolTip();
            }
        }
        if (PhotonNetwork.InRoom)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (currentFocusWindow == tradePanel)
                {
                    if (tradeCheckCntPanel.activeSelf)
                        return;
                    if (!tradeChatInput.isFocused && !tradeChatEnd)
                    {
                        tradeChatInput.ActivateInputField();
                    }
                    tradeChatEnd = false;
                    return;
                }

                if (!chatInput.isFocused && !chatEnd)
                {
                    chatInput.ActivateInputField();
                }
                chatEnd = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            magnifierIcon.SetActive(true);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            magnifierIcon.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            magnifierIcon.SetActive(false);
        }

    }


    #region 세팅
    public void SetUP()
    {
        PlayerGroup = GameObject.Find("Player Group");
        EnemyGroup = GameObject.Find("Enemy Group");
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GetComponent<Canvas>().sortingLayerName = "ui";
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        ResetSkillPanel();
        UpdateSkillPanel();


        UpdatePartyPanel();

        makeProfile();
        characterHealth = DataBase.Instance.myCharacterState.health;
        characterMana = DataBase.Instance.myCharacterState.mana;

        timerText.text = "00:00:00";
        updateAllQuickSlot();
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
        myCharacterProfileUiGroup.transform.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + DataBase.Instance.selectedCharacterSpec.characterLevel.ToString();
    }

    #endregion


    #region 채팅
    public void sendChat()
    {
        if (chatInput.text != "")
        {
            string chat = chatInput.text;
            newNetworkManager.Instance.PV.RPC("sendChatLog", RpcTarget.All, PhotonNetwork.NickName + " : " + chat);
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
    public void CoolDown(string key, float coolingTime)
    {
        StartCoroutine(CoolDownCoroutine(key, coolingTime));
    }
    IEnumerator CoolDownCoroutine(string key, float coolingTime)
    {
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
        skillNameToKey.Clear();
        for (int k = 0; k < quickSkillSlotKeys.Count; k++)
        {
            string key = quickSkillSlotKeys[k].ToLower();
            if (key == "q" || key == "w" || key == "e" || key == "r")
            {
                string skillName = DataBase.Instance.selectedCharacterSpec.skillQuickSlot[key];
                Transform currentSlot = quiclSlotUI.transform.Find(key);
                currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.skillInfoDict[skillName].iconDirectory);
                currentSlot.GetChild(0).GetComponent<Image>().preserveAspect = true;
                currentSlot.GetChild(0).GetComponent<itemslot>().itemName = skillName;
                currentSlot.GetChild(0).GetComponent<itemslot>().isBlank = false;
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = key;
                currentSlot.GetChild(3).GetComponent<TMP_Text>().text = DataBase.Instance.skillInfoDict[skillName].consumeMana.ToString();
                skillNameToKey[skillName] = key;
                CoolDown(key, 0f);
            }
        }
    }

    void useQuickSlot(string key)
    {
        string itemName = DataBase.Instance.selectedCharacterSpec.itemQuickSlot[key];
        if (itemName.IsNullOrEmpty())
            return;
        if (!quickInventory.ContainsKey(itemName))
            return;

        if (quickInventory[itemName].kindCount > 0)
        {
            consumePotion(itemName);
        }
        updateInventory();
        updateThisQuickSlot(key);

    }
    public void updateAllQuickSlot(bool updateSprite = false)
    {
        for (int k = 0; k < quickItemSlotKeys.Count; k++)
        {
            updateThisQuickSlot(quickItemSlotKeys[k], updateSprite);
        }
    }
    void updateThisQuickSlot(string key, bool updateSprtie = false)
    {
        Transform currentSlot = quiclSlotUI.transform.Find(key);
        itemslot slotInfo = currentSlot.GetChild(0).GetComponent<itemslot>();
        string itemName = DataBase.Instance.selectedCharacterSpec.itemQuickSlot[key];
        if (itemName.IsNullOrEmpty())
        {
            slotInfo.isBlank = true;
            slotInfo.itemName = "";
            currentSlot.GetChild(0).GetComponent<Image>().sprite = null;
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.gray;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "";
            return;
        }

        if (updateSprtie)
        {
            currentSlot.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].spriteDirectory);
            currentSlot.GetChild(0).GetComponent<Image>().preserveAspect = true;
        }


        slotInfo.isBlank = false;
        slotInfo.itemName = itemName;
        if (quickInventory.ContainsKey(itemName))
        {
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.white;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[itemName].kindCount.ToString();
        }
        else
        {
            currentSlot.GetChild(0).GetComponent<Image>().color = Color.gray;
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = "0";
        }
    }
    void consumePotion(string itemName)
    {
        DataBase.Instance.myCharacterControl.loseItem(itemName, 1);
        if (DataBase.Instance.itemInfoDict[itemName].recoveryHealth > 0)
            DataBase.Instance.myCharacterState.ProcessSkill(1, DataBase.Instance.itemInfoDict[itemName].recoveryHealth);
        if (DataBase.Instance.itemInfoDict[itemName].recoveryMana > 0)
            DataBase.Instance.myCharacterState.ProcessSkill(5, DataBase.Instance.itemInfoDict[itemName].recoveryMana);
    }

    public void updateInventory()
    {
        inventoryPanel.transform.GetChild(4).GetComponent<TMP_Text>().text = DataBase.Instance.selectedCharacterSpec.money.ToString();
        for (int pos = 0; pos < DataBase.Instance.selectedCharacterSpec.maxInventoryNum; pos++)
        {
            Transform box = inventoryBox.transform.GetChild(pos);
            InventoryItem item = DataBase.Instance.selectedCharacterSpec.inventory[pos];
            if (item == null)
            {
                box.GetChild(2).GetComponent<TMP_Text>().text = "";
                box.GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
                box.GetChild(1).GetComponent<itemslot>().itemName = "";
                box.GetChild(1).GetComponent<itemslot>().slotPos = pos;
                box.GetChild(1).GetComponent<itemslot>().isBlank = true;
            }
            else
            {
                string itemType = DataBase.Instance.itemInfoDict[item.itemName].itemType;
                box.GetChild(2).GetComponent<TMP_Text>().text = item.count.ToString();
                box.GetChild(1).GetComponent<itemslot>().itemName = item.itemName;
                box.GetChild(1).GetComponent<itemslot>().isBlank = false;
                box.GetChild(1).GetComponent<itemslot>().slotPos = pos;
                string iconDir = DataBase.Instance.itemInfoDict[item.itemName].iconDirectory;
                if (iconDir.IsNullOrEmpty())
                    iconDir = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;

                box.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(iconDir);
                box.GetChild(1).GetComponent<Image>().preserveAspect = true;
                box.GetChild(1).GetComponent<Image>().color = Color.white;
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
            if (currentFocusWindow == storePanel)
            {
                if (storeBuyPanel.activeSelf)
                {
                    storeBuyPanel.SetActive(false);
                    openedWindows.Remove(storeBuyPanel);
                }
                if (storeSellPanel.activeSelf)
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

    void ShowUserInteractionPanel(string userName)
    {
        userInteractionPanel.transform.GetChild(0).GetChild(1).name = userName;
        userInteractionPanel.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = inGameUserList[userName].nick;
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        userInteractionPanel.transform.position = new Vector3(mousePoint.x, mousePoint.y, 0);

        updateCurrentFocusWindow(userInteractionPanel);
    }
    #endregion


    #region 마우스 이벤트
    public void EnterToolTip()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        if (results.Count > 0)
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
            isHoverToolTip = false;
            return;
        }
        if (toolTipPanel.activeSelf)
        {
            toolTipPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 185 + toolTipPanel.transform.GetChild(1).GetChild(3).GetComponent<RectTransform>().sizeDelta.y);
            return;
        }

        itemslot slotInfo = hoverObject.GetComponent<itemslot>();
        if (slotInfo.isBlank)
        {
            return;
        }
        string toolTipName = slotInfo.itemName;
        string toolTipContent = "";
        string toolTipSummary = "";
        string iconDir = "";
        float pivotX = 0.5f;
        float pivotY = 0f;

        if (slotInfo.slotType == "skill" || slotInfo.slotType == "quick skill")
        {
            toolTipContent = DataBase.Instance.skillInfoDict[toolTipName].description;

            toolTipContent = toolTipContent.Replace("(sumDeal)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatDeal +
                DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerSkillLevel * DataBase.Instance.selectedCharacterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatDeal)", DataBase.Instance.skillInfoDict[toolTipName].flatDeal.ToString());
            toolTipContent = toolTipContent.Replace("(dealIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(dealIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(sumHeal)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatHeal +
                DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerSkillLevel * DataBase.Instance.selectedCharacterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatHeal)", DataBase.Instance.skillInfoDict[toolTipName].flatHeal.ToString());
            toolTipContent = toolTipContent.Replace("(healIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(healIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(sumShield)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatShield +
                DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerSkillLevel * DataBase.Instance.selectedCharacterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatShield)", DataBase.Instance.skillInfoDict[toolTipName].flatShield.ToString());
            toolTipContent = toolTipContent.Replace("(shieldIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(shieldIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(sumPower)",
                (DataBase.Instance.skillInfoDict[toolTipName].flatPower +
                DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerSkillLevel * DataBase.Instance.selectedCharacterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerPower * DataBase.Instance.myCharacterState.power).ToString());
            toolTipContent = toolTipContent.Replace("(flatPower)", DataBase.Instance.skillInfoDict[toolTipName].flatPower.ToString());
            toolTipContent = toolTipContent.Replace("(powerIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(powerIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(coolDown)", DataBase.Instance.skillInfoDict[toolTipName].coolDown.ToString());
            toolTipContent = toolTipContent.Replace("(consumeMana)", DataBase.Instance.skillInfoDict[toolTipName].consumeMana.ToString());
            toolTipContent = toolTipContent.Replace("(duration)", DataBase.Instance.skillInfoDict[toolTipName].duration.ToString());

            iconDir = DataBase.Instance.skillInfoDict[toolTipName].iconDirectory;
            toolTipSummary = string.Format("마스터 레벨 : {0}\n스킬 레벨 : {1}", DataBase.Instance.skillInfoDict[toolTipName].maxLevel, DataBase.Instance.selectedCharacterSpec.skillLevel[toolTipName]);

        }


        else if (DataBase.Instance.itemInfoDict[slotInfo.itemName].itemType == "material" || DataBase.Instance.itemInfoDict[slotInfo.itemName].itemType == "potion")
        {
            toolTipContent = DataBase.Instance.itemInfoDict[toolTipName].description;

            toolTipContent = toolTipContent.Replace("(health)", DataBase.Instance.itemInfoDict[toolTipName].recoveryHealth.ToString());
            toolTipContent = toolTipContent.Replace("(mana)", DataBase.Instance.itemInfoDict[toolTipName].recoveryMana.ToString());

            iconDir = DataBase.Instance.itemInfoDict[toolTipName].iconDirectory;
            if (iconDir.IsNullOrEmpty())
                iconDir = DataBase.Instance.itemInfoDict[toolTipName].spriteDirectory;

            toolTipSummary = string.Format("판매 가격 : {0}", DataBase.Instance.itemInfoDict[toolTipName].sellPrice.ToString());
        }
        else
        {
            int pos = slotInfo.slotPos;
            int reinforce = 0;
            if (slotInfo.slotType == "inven" || slotInfo.slotType == "storeInven")
            {
                reinforce = DataBase.Instance.selectedCharacterSpec.inventory[pos].reinforce;
            }
            else if (slotInfo.slotType == "equip")
            {
                reinforce = DataBase.Instance.selectedCharacterSpec.equipment[-pos - 1].reinforce;
            }
            else if (slotInfo.slotType == "store")
            {
                reinforce = 0;
            }
            else if (slotInfo.slotType == "enchant")
            {
                if (pos < 0)
                    reinforce = DataBase.Instance.selectedCharacterSpec.equipment[-pos - 1].reinforce;
                else
                    reinforce = DataBase.Instance.selectedCharacterSpec.inventory[pos].reinforce;
            }
            else if (slotInfo.slotType == "opTrade" || slotInfo.slotType == "trade")
            {
                reinforce = pos;
            }


            toolTipSummary = string.Format("공격력 +{0}", DataBase.Instance.CalEnchantPower(toolTipName, reinforce) + DataBase.Instance.itemInfoDict[toolTipName].increasePower);
            iconDir = DataBase.Instance.itemInfoDict[toolTipName].iconDirectory;
            if (iconDir.IsNullOrEmpty())
                iconDir = DataBase.Instance.itemInfoDict[toolTipName].spriteDirectory;
            toolTipContent = DataBase.Instance.itemInfoDict[toolTipName].description;
            if (reinforce > 0)
                toolTipName += string.Format(" (+{0})", reinforce);
        }


        toolTipPanel.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = toolTipName;
        toolTipPanel.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(iconDir);
        toolTipPanel.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>().preserveAspect = true;
        toolTipPanel.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = toolTipSummary;
        toolTipPanel.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().text = toolTipContent;
        float worldx = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0f)).x * 0.75f;
        if (hoverObject.transform.position.x > worldx)
            pivotX = 1;
        else if (hoverObject.transform.position.x < -worldx)
            pivotX = 0f;
        else
            pivotX = 0.5f;
        if (hoverObject.transform.position.y > 0)
            pivotY = 1f;
        else
            pivotY = 0f;
        toolTipPanel.GetComponent<RectTransform>().pivot = new Vector2(pivotX, pivotY);
        toolTipPanel.transform.position = hoverObject.transform.position;
        toolTipPanel.SetActive(true);
    }


    public void DragItemBegin(BaseEventData eventData)
    {
        PointerEventData pointer_data = (PointerEventData)eventData;
        dragObject = pointer_data.pointerDrag;
        string slotType = dragObject.GetComponent<itemslot>().slotType;
        if (dragObject == null)
            return;
        if (dragObject.GetComponent<itemslot>().isBlank)
        {
            dragObject = null;
            return;
        }
        //dragObjectOriginPos = dragObject.transform.localPosition;

        draggingItem.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = dragObject.GetComponent<Image>().sprite;
        draggingItem.transform.GetChild(0).GetChild(0).GetComponent<Image>().preserveAspect = true;

        isHoverToolTip = false;
        hoverTime = 0;
    }
    public void DragItemIng(BaseEventData eventData)
    {
        if (dragObject == null)
            return;
        draggingItem.SetActive(true);
        PointerEventData pointer_data = (PointerEventData)eventData;
        Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        draggingItem.transform.position = new Vector3(currentMousePos.x, currentMousePos.y, -5f);

        isHoverToolTip = false;
        hoverTime = 0;
    }
    public void DragItemEnd(BaseEventData eventData)
    {
        PointerEventData pointer_data = (PointerEventData)eventData;
        if (dragObject == null)
            return;
        if (pointer_data.pointerCurrentRaycast.gameObject == null)
        {
            DragItemDone();
            return;
        }
        Debug.Log(pointer_data.pointerCurrentRaycast.gameObject.name);
        itemslot dragItemSlot = dragObject.GetComponent<itemslot>();
        itemslot desSlot = pointer_data.pointerCurrentRaycast.gameObject.GetComponent<itemslot>();
        if (desSlot != null)
        {
            if (desSlot.slotType == "quick item")
            {
                if (dragItemSlot.slotType == "inven" || dragItemSlot.slotType == "quick item")
                {
                    if (DataBase.Instance.itemInfoDict[dragItemSlot.itemName].itemType != "potion")
                    {
                        DragItemDone();
                        return;
                    }
                    string dragItemName = dragItemSlot.itemName;
                    string dragItemPos = dragItemSlot.slotPos.ToString();

                    string desSlotName = desSlot.itemName;
                    string desSlotPos = desSlot.slotPos.ToString();

                    DataBase.Instance.selectedCharacterSpec.itemQuickSlot[desSlotPos] = dragItemName;
                    if (dragItemSlot.slotType == "quick item")
                    {
                        DataBase.Instance.selectedCharacterSpec.itemQuickSlot[dragItemPos] = desSlotName;
                    }
                    updateAllQuickSlot(true);
                }
            }
            else if (desSlot.slotType == "quick skill")
            {
                if (dragItemSlot.slotType == "skill" || dragItemSlot.slotType == "quick skill")
                {
                    string dragSkillKey = skillNameToKey[dragItemSlot.itemName];
                    string dragSKillName = dragItemSlot.itemName;

                    string desSkillKey = skillNameToKey[desSlot.itemName];
                    string desSKillName = desSlot.itemName;

                    if (!dragSkillKey.IsNullOrEmpty())
                        DataBase.Instance.selectedCharacterSpec.skillQuickSlot[dragSkillKey] = desSKillName;
                    if (!desSkillKey.IsNullOrEmpty())
                        DataBase.Instance.selectedCharacterSpec.skillQuickSlot[desSkillKey] = dragSKillName;
                    setKeyMap();
                }
            }
            else if (desSlot.slotType == "enchant")
            {
                if (isEnchanting)
                {
                    DragItemDone();
                    return;
                }
                if (dragItemSlot.slotType != "inven" && dragItemSlot.slotType != "equip")
                {
                    DragItemDone();
                    return;
                }
                else if (DataBase.Instance.itemInfoDict[dragItemSlot.itemName].itemType == "potion" || DataBase.Instance.itemInfoDict[dragItemSlot.itemName].itemType == "material")
                {
                    DragItemDone();
                    return;
                }
                desSlot.itemName = dragItemSlot.itemName;
                desSlot.slotPos = dragItemSlot.slotPos;
                desSlot.isBlank = false;
                UpdateEnchantPanel();
            }
            else if (desSlot.slotType == "inven")
            {
                if (dragItemSlot.slotType == "inven") // 인벤토리 내 이동 스왑
                {
                    if (desSlot.slotPos != dragItemSlot.slotPos)
                    {
                        if (DataBase.Instance.selectedCharacterSpec.inventory[desSlot.slotPos] == null)
                        {
                            string dragItemName = dragItemSlot.itemName;
                            int dragReinforce = DataBase.Instance.selectedCharacterSpec.inventory[dragItemSlot.slotPos].reinforce;
                            int dragCnt = DataBase.Instance.selectedCharacterSpec.inventory[dragItemSlot.slotPos].count;
                            DataBase.Instance.myCharacterControl.loseItem(dragItemName, dragCnt, dragItemSlot.slotPos);
                            DataBase.Instance.myCharacterControl.getItem(new Item { itemName = dragItemName, itemCount = dragCnt, reinforce = dragReinforce }, false, desSlot.slotPos);
                        }
                        else
                        {
                            string desItemName = desSlot.itemName;
                            int desReinforce = DataBase.Instance.selectedCharacterSpec.inventory[desSlot.slotPos].reinforce;
                            int desCnt = DataBase.Instance.selectedCharacterSpec.inventory[desSlot.slotPos].count;

                            string dragItemName = dragItemSlot.itemName;
                            int dragReinforce = DataBase.Instance.selectedCharacterSpec.inventory[dragItemSlot.slotPos].reinforce;
                            int dragCnt = DataBase.Instance.selectedCharacterSpec.inventory[dragItemSlot.slotPos].count;

                            DataBase.Instance.myCharacterControl.loseItem(desItemName, desCnt, desSlot.slotPos);
                            DataBase.Instance.myCharacterControl.loseItem(dragItemName, dragCnt, dragItemSlot.slotPos);

                            DataBase.Instance.myCharacterControl.getItem(new Item { itemName = desItemName, itemCount = desCnt, reinforce = desReinforce }, false, dragItemSlot.slotPos);
                            DataBase.Instance.myCharacterControl.getItem(new Item { itemName = dragItemName, itemCount = dragCnt, reinforce = dragReinforce }, false, desSlot.slotPos);

                        }

                        updateInventory();
                    }

                }
                else if (dragItemSlot.slotType == "equip") // 장비 -> 인벤토리
                {
                    if (desSlot.isBlank) // 빈곳이면 장착해제
                    {
                        int equipPos = -dragItemSlot.slotPos - 1;
                        int invenPos = desSlot.slotPos;
                        InventoryItem equip = DataBase.Instance.selectedCharacterSpec.equipment[equipPos];
                        DataBase.Instance.myCharacterControl.getItem(new Item { reinforce = equip.reinforce, itemName = equip.itemName, itemCount = 1 }, false, invenPos);
                        DataBase.Instance.selectedCharacterSpec.equipment.Remove(equip);


                        DataBase.Instance.myCharacterState.equipItem();
                        UpdateEquipmentPanel();
                        updateInventory();

                    }
                    else if (DataBase.Instance.itemInfoDict[desSlot.itemName].itemType == dragItemSlot.transform.parent.name ||
                        (DataBase.Instance.itemInfoDict[desSlot.itemName].itemType.Contains("weapon") && dragItemSlot.transform.parent.name == "weapon"))
                    { //같은 종류면 스왑
                        int equipPos = -dragItemSlot.slotPos - 1;
                        int invenPos = desSlot.slotPos;

                        string invenName = DataBase.Instance.selectedCharacterSpec.inventory[invenPos].itemName;
                        int invenRein = DataBase.Instance.selectedCharacterSpec.inventory[invenPos].reinforce;


                        InventoryItem equip = DataBase.Instance.selectedCharacterSpec.equipment[equipPos];

                        DataBase.Instance.myCharacterControl.loseItem(invenName, 1, invenPos);
                        DataBase.Instance.myCharacterControl.getItem(new Item { reinforce = equip.reinforce, itemName = equip.itemName, itemCount = 1 }, false, invenPos);
                        DataBase.Instance.selectedCharacterSpec.equipment.Remove(equip);
                        DataBase.Instance.selectedCharacterSpec.equipment.Add(new InventoryItem { reinforce = invenRein, itemName = invenName, count = 1 });
                        DataBase.Instance.myCharacterState.equipItem();
                        UpdateEquipmentPanel();
                        updateInventory();
                    }
                    else
                    {
                        DragItemDone();
                        return;
                    }
                }
            }
            else if (desSlot.slotType == "equip")
            {
                if (dragItemSlot.slotType == "inven")
                {
                    if (DataBase.Instance.itemInfoDict[dragItemSlot.itemName].itemType == desSlot.transform.parent.name ||
                        (DataBase.Instance.itemInfoDict[dragItemSlot.itemName].itemType.Contains("weapon") && desSlot.transform.parent.name == "weapon"))
                    {//같은 종류 중
                        int invenPos = dragItemSlot.slotPos;
                        string invenName = DataBase.Instance.selectedCharacterSpec.inventory[invenPos].itemName;
                        int invenRein = DataBase.Instance.selectedCharacterSpec.inventory[invenPos].reinforce;
                        if (desSlot.isBlank)
                        {// 빈곳이면 장착
                            DataBase.Instance.selectedCharacterSpec.equipment.Add(new InventoryItem { reinforce = invenRein, itemName = invenName, count = 1 });
                            DataBase.Instance.myCharacterControl.loseItem(invenName, 1, invenPos);

                        }
                        else
                        {//아니면 스왑
                            int equipPos = -desSlot.slotPos - 1;

                            InventoryItem equip = DataBase.Instance.selectedCharacterSpec.equipment[equipPos];

                            DataBase.Instance.myCharacterControl.loseItem(invenName, 1, invenPos);
                            DataBase.Instance.myCharacterControl.getItem(new Item { reinforce = equip.reinforce, itemName = equip.itemName, itemCount = 1 }, false, invenPos);
                            DataBase.Instance.selectedCharacterSpec.equipment.Remove(equip);
                            DataBase.Instance.selectedCharacterSpec.equipment.Add(new InventoryItem { reinforce = invenRein, itemName = invenName, count = 1 });
                        }
                        DataBase.Instance.myCharacterState.equipItem();
                        UpdateEquipmentPanel();
                        updateInventory();
                    }
                    else
                    {
                        DragItemDone();
                        return;
                    }
                }
            }
            else if (desSlot.slotType == "trade")
            {
                if (opTradeBox.transform.parent.GetChild(4).gameObject.activeSelf || myTradeBox.transform.parent.GetChild(4).gameObject.activeSelf)
                {
                    DragItemDone();
                    return;
                }
                if (dragItemSlot.slotType == "inven")
                {
                    int slotPos = int.Parse(desSlot.transform.parent.parent.name);
                    popTradeCheckCnt(dragItemSlot.itemName, dragItemSlot.slotPos, slotPos);
                }
            }
            else
            {
                Debug.LogError("wrong slot");
                DragItemDone();
                return;
            }
        }
        DragItemDone();
    }

    void DragItemDone()
    {
        //dragObject.transform.localPosition = dragObjectOriginPos;
        draggingItem.SetActive(false);

        dragObject = null;
        isHoverToolTip = false;
        hoverTime = 0;
    }


    public void ClickItem(BaseEventData eventData)
    {
        PointerEventData pointer_data = (PointerEventData)eventData;
        if (pointer_data.pointerClick == null)
            return;
        itemslot itemInfo = pointer_data.pointerClick.GetComponent<itemslot>();
        if (itemInfo.isBlank)
            return;
        int pos = itemInfo.slotPos;

        if (Time.time - itemDoubleClickTimer < 0.25f)
        {
            if (pos < 0)
            {
                pos = -pos - 1;
                InventoryItem item = DataBase.Instance.selectedCharacterSpec.equipment[pos];
                string itemName = item.itemName;
                int reinforce = item.reinforce;
                DataBase.Instance.selectedCharacterSpec.equipment.Remove(item);
                DataBase.Instance.myCharacterControl.getItem(new Item { itemName = itemName, itemCount = 1, reinforce = reinforce }, false);
                DataBase.Instance.myCharacterState.equipItem();
                updateInventory();
                UpdateEquipmentPanel();
            }
            else
            {
                InventoryItem item = DataBase.Instance.selectedCharacterSpec.inventory[pos];
                string itemName = item.itemName;
                int reinforce = item.reinforce;
                string itemType = DataBase.Instance.itemInfoDict[item.itemName].itemType;
                if (itemType == "material")
                {
                    return;
                }
                else if (itemType == "potion")
                {
                    consumePotion(itemName);
                }
                else
                {
                    itemslot equipSlot;
                    if (itemType.Contains("weapon"))
                        equipSlot = equipmentPanel.transform.GetChild(2).Find("weapon").GetChild(0).GetComponent<itemslot>();
                    else
                        equipSlot = equipmentPanel.transform.GetChild(2).Find(itemType).GetChild(0).GetComponent<itemslot>();
                    if (equipSlot.isBlank)
                    {
                        DataBase.Instance.myCharacterControl.loseItem(itemName, 1, pos);
                        DataBase.Instance.selectedCharacterSpec.equipment.Add(new InventoryItem { itemName = itemName, count = 1, reinforce = reinforce });
                    }
                    else
                    {
                        InventoryItem equip = DataBase.Instance.selectedCharacterSpec.equipment[-equipSlot.slotPos - 1];

                        DataBase.Instance.myCharacterControl.loseItem(itemName, 1, pos);
                        DataBase.Instance.myCharacterControl.getItem(new Item { reinforce = equip.reinforce, itemName = equip.itemName, itemCount = 1 }, false, pos);
                        DataBase.Instance.selectedCharacterSpec.equipment.Remove(equip);
                        DataBase.Instance.selectedCharacterSpec.equipment.Add(new InventoryItem { reinforce = reinforce, itemName = itemName, count = 1 });
                    }
                    DataBase.Instance.myCharacterState.equipItem();
                    updateInventory();
                    UpdateEquipmentPanel();
                }
            }
        }
        else
        {
            itemDoubleClickTimer = Time.time;
        }

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
        List<string> skillName = DataBase.Instance.selectedCharacterSpec.skillQuickSlot.SD_Values;
        for (int k = 0; k < skillName.Count; k++)
        {
            string name = skillName[k];
            if (name.Contains("normal"))
                continue;
            Transform _skillBox = skillBox.transform.Find(name);
            if (_skillBox == null)
            {
                GameObject newSkill = Instantiate(skillInfo);
                newSkill.name = "skill " + name;
                newSkill.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.skillInfoDict[name].iconDirectory);
                newSkill.transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
                itemslot _itemSlot = newSkill.transform.GetChild(1).GetComponent<itemslot>();
                _itemSlot.itemName = name;
                _itemSlot.slotType = "skill";
                _itemSlot.slotPos = k;
                _itemSlot.isBlank = false;
                newSkill.transform.GetChild(2).GetComponent<TMP_Text>().text = name;
                string max_level = DataBase.Instance.skillInfoDict[name].maxLevel.ToString();
                string current_level = DataBase.Instance.selectedCharacterSpec.skillLevel[name].ToString();
                newSkill.transform.GetChild(3).GetComponent<TMP_Text>().text = current_level + " / " + max_level;
                newSkill.transform.SetParent(skillBox.transform, false);
                newSkill.transform.localPosition = Vector3.zero;
                newSkill.transform.localScale = Vector3.one;
                newSkill.gameObject.SetActive(true);
            }
            else
            {
                string max_level = DataBase.Instance.skillInfoDict[name].maxLevel.ToString();
                string current_level = DataBase.Instance.selectedCharacterSpec.skillLevel[name].ToString();
                _skillBox.transform.GetChild(3).GetComponent<TMP_Text>().text = current_level + " / " + max_level;
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
            if (stageTime >= DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].timeLimit[DataBase.Instance.currentDungeonLevel])
                break;
            yield return null;
        }
        EndGame("time out");


    }
    public void EndGame(string condition)
    {
        string title = null;
        string content = null;
        if (timer != null)
            StopCoroutine(timer);
        if (condition == "time out")
        {
            title = "타임아웃";
            gameOverPanel.transform.GetChild(0).GetComponent<Image>().color = failColor;
            if (DataBase.Instance.currentStage - 1 == DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].monsterInfoList.Count)
            {
                content = string.Format("남은 체력\n{0}\n\n소요시간\n{1}초\n\n인 원\n", bossCurrentHealthText.text, DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].timeLimit[DataBase.Instance.currentDungeonLevel].ToString());
            }
            else
            {
                content = string.Format("스테이지\n{0}\n\n소요시간\n{1}초\n\n인 원\n", DataBase.Instance.currentStage.ToString(), DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].timeLimit[DataBase.Instance.currentDungeonLevel].ToString());
            }
            gameOverPanel.transform.GetChild(3).gameObject.SetActive(true);
            gameOverPanel.transform.GetChild(4).gameObject.SetActive(true);
        }
        else if (condition == "clear")
        {
            title = "클리어";
            gameOverPanel.transform.GetChild(0).GetComponent<Image>().color = succesColor;
            content = string.Format("클리어 시간\n{0}초\n\n인 원\n", stageTime.ToString());
            gameOverPanel.transform.GetChild(3).gameObject.SetActive(false);
            gameOverPanel.transform.GetChild(4).gameObject.SetActive(false);
        }
        else if (condition == "all death")
        {
            title = "실패";
            gameOverPanel.transform.GetChild(0).GetComponent<Image>().color = failColor;
            if (DataBase.Instance.currentStage - 1 == DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].monsterInfoList.Count)
            {
                content = string.Format("남은 체력\n{0}\n\n소요시간\n{1}초\n\n인 원\n", bossCurrentHealthText.text, stageTime.ToString());
            }
            else
            {
                content = string.Format("스테이지\n{0}\n\n소요시간\n{1}초\n\n인 원\n", DataBase.Instance.currentStage.ToString(), stageTime.ToString());
            }
            gameOverPanel.transform.GetChild(3).gameObject.SetActive(true);
            gameOverPanel.transform.GetChild(4).gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("wrong condition");
            return;
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


        if (DataBase.Instance.isCaptain)
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
        newNetworkManager.Instance.PV.RPC("ReGame", RpcTarget.All);
    }
    public void ClickGoToVillageButton()
    {
        newNetworkManager.Instance.PV.RPC("GoToVillage", RpcTarget.All);
    }

    public void EnterDungeonPop()
    {
        updateCurrentFocusWindow(enterDungeonPanel);
    }

    public void ClickEnterDungeonButton(int level)
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string dungeonName = current_clicked_button.transform.parent.name;
        newNetworkManager.Instance.movePortal(dungeonName, level);
        enterDungeonPanel.SetActive(false);
    }


    public void LoadingPop()
    {
        loadingPanel.SetActive(true);
        int partyMemberNum = DataBase.Instance.myPartyMemNum;
        for (int k = 0; k < 3; k++)
        {
            if (k < partyMemberNum)
            {
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                Transform profile = Instantiate(partyMemberBox.transform.GetChild(k).GetChild(1).GetChild(0).transform);
                profile.parent = loadingPanel.transform.GetChild(0).GetChild(k).GetChild(1).transform;
                profile.localScale = new Vector3(150f, 150f);
                profile.localPosition = new Vector3(0, -35f);
                string nick = partyMemberBox.transform.GetChild(k).GetChild(2).GetComponent<TMP_Text>().text;
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(2).GetComponent<TMP_Text>().text = nick;
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(3).GetComponent<TMP_Text>().text = partyMemberBox.transform.GetChild(k).GetChild(3).GetComponent<TMP_Text>().text;
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(4).GetComponent<TMP_Text>().text = partyMemberBox.transform.GetChild(k).GetChild(4).GetComponent<TMP_Text>().text;
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(5).gameObject.SetActive(partyMemberBox.transform.GetChild(k).GetChild(5).gameObject.activeSelf);
            }
            else
            {
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, (10f / 255f));
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, (10f / 255f));
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(2).GetComponent<TMP_Text>().text = "";
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(3).GetComponent<TMP_Text>().text = "";
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(4).GetComponent<TMP_Text>().text = "";
                loadingPanel.transform.GetChild(0).GetChild(k).GetChild(5).gameObject.SetActive(false);
            }
        }
        StartCoroutine(updateLoadingPanel(partyMemberNum));
    }

    IEnumerator updateLoadingPanel(int partyMemNum)
    {
        while (!DataBase.Instance.isInDungeon)
        {
            yield return null;
        }
        while (partyMemNum != PlayerGroup.transform.childCount)
        {
            yield return null;
        }
        loadingPanel.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().SpawnMonster();
        }
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
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            partyPanel.transform.GetChild(7).GetChild(1).GetComponent<Button>().interactable = false;
        }
        else
        {
            partyPanel.transform.GetChild(7).GetChild(1).GetComponent<Button>().interactable = true;
        }
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
                        newMember.transform.GetChild(5).gameObject.SetActive(true);
                    else
                        newMember.transform.GetChild(5).gameObject.SetActive(false);
                    newMember.transform.GetChild(3).GetComponent<TMP_Text>().text = "Lv. " + playerInfo.level.ToString();
                    newMember.transform.GetChild(4).GetComponent<TMP_Text>().text = "직업: " + playerInfo.roll;
                    newMember.transform.GetChild(6).name = memberName;
                    if (!DataBase.Instance.isCaptain)
                        newMember.transform.GetChild(6).GetComponent<Button>().interactable = false;
                    else if (memberName == DataBase.Instance.myCharacter.name)
                        newMember.transform.GetChild(6).GetComponent<Button>().interactable = false;
                    else if (DataBase.Instance.currentMapType == "dungeon")
                        newMember.transform.GetChild(6).GetComponent<Button>().interactable = false;
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
            newParty.transform.GetChild(0).GetComponent<TMP_Text>().text = "파티장: " + inGameUserList[partyInfo.captainName].nick;
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
        if (DataBase.Instance.currentMapType == "dungeon")
            return;
        string partyName = partyMakeNameInput.text;
        if (partyMakeNameInput.text.IsNullOrEmpty())
            partyName = "파티 고고";
        DataBase.Instance.isCaptain = true;
        DataBase.Instance.myPartyCaptainName = DataBase.Instance.myCharacter.name;
        DataBase.Instance.myPartyName = partyName;
        DataBase.Instance.myCharacterState.updateParty();
        newNetworkManager.Instance.PV.RPC("UpdateParty", RpcTarget.All);
    }

    public void ClickAcceptPartyInviteButton()
    {
        UpdateInGameUser();
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string captainName = current_clicked_button.name;
        if (allPartys[captainName].partyMembersNickName.Count < 3)
        {
            DataBase.Instance.isCaptain = false;
            DataBase.Instance.myPartyCaptainName = captainName;
            DataBase.Instance.myPartyName = allPartys[captainName].partyName;
            DataBase.Instance.myCharacterState.updateParty();
            newNetworkManager.Instance.PV.RPC("UpdateParty", RpcTarget.All);
        }
        else
        {
            popInfo("파티 정원을 초과해 참가할 수 없습니다.");
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
            newNetworkManager.Instance.PV.RPC("acceptJoinParty", inGameUserList[current_clicked_button.name].PV.Owner, DataBase.Instance.myCharacter.name, DataBase.Instance.myPartyName);
        }
        else
        {
            popInfo("파티 정원을 초과합니다.");
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
                    if (newCaptainName.IsNullOrEmpty())
                        newCaptainName = memberName;
                    newNetworkManager.Instance.PV.RPC("ChangeCaptain", inGameUserList[memberName].PV.Owner, newCaptainName);
                }
            }
        }
        DataBase.Instance.isCaptain = false;
        DataBase.Instance.myPartyCaptainName = "";
        DataBase.Instance.myPartyName = "";
        DataBase.Instance.myCharacterState.updateParty();
        newNetworkManager.Instance.PV.RPC("UpdateParty", RpcTarget.All);
    }

    public void ClickKickPartyMemberButton()
    {
        if (DataBase.Instance.myPartyCaptainName != DataBase.Instance.myCharacter.name)
            return;
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        newNetworkManager.Instance.PV.RPC("kickPartyMember", inGameUserList[current_clicked_button.name].PV.Owner);
    }



    public void ClickRejectPartyInviteButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string captainName = current_clicked_button.name;
        newNetworkManager.Instance.PV.RPC("sendInfo", inGameUserList[captainName].PV.Owner, DataBase.Instance.myCharacterState.nick + "님이 파티 초대를 거절하였습니다.");
        invitePartyPanel.SetActive(false);
        openedWindows.Remove(invitePartyPanel);
        updateCurrentFocusWindow();
    }



    public void ClickRejectJoinPartyRequestButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        string requesterName = current_clicked_button.name;
        newNetworkManager.Instance.PV.RPC("sendInfo", inGameUserList[requesterName].PV.Owner, DataBase.Instance.myCharacterState.nick + "님이 파티 신청을 거절하였습니다.");
        joinPartyRequestPanel.SetActive(false);
        openedWindows.Remove(joinPartyRequestPanel);
        updateCurrentFocusWindow();
    }


    public void ClickPartyInviteButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        newNetworkManager.Instance.PV.RPC("sendAndReceiveInviteParty",
            inGameUserList[current_clicked_button.name].PV.Owner,
            allPartys[DataBase.Instance.myPartyCaptainName].partyName,
            DataBase.Instance.myCharacter.name);
    }

    public void ClickPartyJoinRequsetButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        newNetworkManager.Instance.PV.RPC("sendAndReceiveJoinRequestParty",
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
        invitePartyPanel.transform.GetChild(2).GetChild(2).name = captain;
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
        joinPartyRequestPanel.transform.GetChild(2).GetChild(2).name = fromWho;
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
        storeBuyPanel.SetActive(false);
        storeSellPanel.SetActive(false);
        storeBuyPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
        storeSellPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
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
            sellingItem.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory);
            sellingItem.transform.GetChild(1).GetChild(0).GetComponent<Image>().preserveAspect = true;
            sellingItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().slotType = "store";
            sellingItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().itemName = item.itemName;
            sellingItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().isBlank = true;
            sellingItem.transform.GetChild(2).GetComponent<TMP_Text>().text = item.itemName;
            sellingItem.transform.GetChild(3).GetComponent<TMP_Text>().text = DataBase.Instance.itemInfoDict[item.itemName].buyPrice.ToString();
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
        storeInvenBox.transform.parent.GetChild(1).GetComponent<TMP_Text>().text = DataBase.Instance.selectedCharacterSpec.money.ToString();
        foreach (string itemName in quickInventory.Keys)
        {

            if (DataBase.Instance.itemInfoDict[itemName].itemType == "material" || DataBase.Instance.itemInfoDict[itemName].itemType == "potion")
            {
                GameObject invenItem = Instantiate(storeInvenItemInfo);
                invenItem.name = itemName;
                invenItem.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].spriteDirectory);
                invenItem.transform.GetChild(1).GetChild(0).GetComponent<Image>().preserveAspect = true;
                invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().itemName = itemName;
                invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().slotPos = -1;
                invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().isBlank = false;
                invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().slotType = "storeInven";
                invenItem.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = quickInventory[itemName].kindCount.ToString();
                invenItem.transform.GetChild(2).GetComponent<TMP_Text>().text = itemName;
                invenItem.transform.GetChild(3).GetComponent<TMP_Text>().text = DataBase.Instance.itemInfoDict[itemName].sellPrice.ToString();
                invenItem.transform.parent = storeInvenBox.transform;
                invenItem.transform.localScale = Vector3.one;
                invenItem.transform.localPosition = Vector3.zero;
                invenItem.SetActive(true);
            }
            else
            {
                foreach (int pos in quickInventory[itemName].position)
                {
                    InventoryItem item = DataBase.Instance.selectedCharacterSpec.inventory[pos];
                    GameObject invenItem = Instantiate(storeInvenItemInfo);
                    string iconDir = DataBase.Instance.itemInfoDict[itemName].iconDirectory;
                    if (iconDir.IsNullOrEmpty())
                        iconDir = DataBase.Instance.itemInfoDict[itemName].spriteDirectory;
                    invenItem.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(iconDir);
                    invenItem.transform.GetChild(1).GetChild(0).GetComponent<Image>().preserveAspect = true;
                    invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().itemName = itemName;
                    invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().slotPos = pos;
                    invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().isBlank = false;
                    invenItem.transform.GetChild(1).GetChild(0).GetComponent<itemslot>().slotType = "storeInven";
                    invenItem.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = item.count.ToString();
                    invenItem.transform.GetChild(2).GetComponent<TMP_Text>().text = itemName;
                    invenItem.transform.GetChild(3).GetComponent<TMP_Text>().text = DataBase.Instance.itemInfoDict[itemName].sellPrice.ToString();
                    invenItem.transform.parent = storeInvenBox.transform;
                    invenItem.transform.localScale = Vector3.one;
                    invenItem.transform.localPosition = Vector3.zero;
                    invenItem.SetActive(true);
                }
            }


        }
    }
    public void ClickStoreItem(bool buy)
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        itemslot item = current_clicked_button.transform.GetChild(1).GetChild(0).GetComponent<itemslot>();
        if (buy)
        {
            if (Time.time - storeBuyDoubleClickTimer < 0.25f)
            {
                storeBuyPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                storeBuyPanel.SetActive(true);
                storeBuyPanel.transform.GetChild(2).name = item.itemName;
                storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = "1";
                storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().Select();
            }
            else
            {
                storeBuyDoubleClickTimer = Time.time;
            }
        }
        else
        {
            if (Time.time - storeSellDoubleClickTimer < 0.25f)
            {
                storeSellPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
                storeSellPanel.SetActive(true);
                storeSellPanel.transform.GetChild(2).name = item.itemName;
                if (DataBase.Instance.itemInfoDict[item.itemName].itemType == "material" || DataBase.Instance.itemInfoDict[item.itemName].itemType == "potion")
                {
                    storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = quickInventory[item.itemName].kindCount.ToString();
                }
                else
                {
                    storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = "1";
                }
                storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().Select();
                storeSellPanel.transform.GetChild(2).GetChild(1).name = item.slotPos.ToString();
            }
            else
            {
                storeSellDoubleClickTimer = Time.time;
            }
        }
    }

    public void ClickSellButton()
    {
        if (storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text.IsNullOrEmpty())
            return;
        string sellItemName = storeSellPanel.transform.GetChild(2).name;
        int sellItemCnt = int.Parse(storeSellPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text);
        int invenPos = int.Parse(storeSellPanel.transform.GetChild(2).GetChild(1).name);
        if (sellItemCnt <= 0)
        {
            storeSellPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = "0보다 큰 수를 입력해주세요.";
            storeSellPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
            return;
        }
        if (DataBase.Instance.itemInfoDict[sellItemName].itemType == "material" || DataBase.Instance.itemInfoDict[sellItemName].itemType == "potion")
        {
            if (sellItemCnt > DataBase.Instance.selectedCharacterSpec.inventory[invenPos].count)
            {
                storeSellPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text =
                    string.Format("아이템이 {0}개 보다 부족합니다.", sellItemCnt);
                storeSellPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
                return;
            }
        }
        else
        {
            if (sellItemCnt > DataBase.Instance.selectedCharacterSpec.inventory[invenPos].count)
            {
                storeSellPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = "1개만 판매할 수 있습니다.";
                storeSellPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
                return;
            }
        }

        int sellMoney = sellItemCnt * DataBase.Instance.itemInfoDict[sellItemName].sellPrice;
        DataBase.Instance.selectedCharacterSpec.money += sellMoney;
        DataBase.Instance.myCharacterControl.loseItem(sellItemName, sellItemCnt, invenPos);
        updateInventory();
        UpdateStoreInventory();
        storeSellPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
        storeSellPanel.SetActive(false);
    }

    public void ClickBuyButton()
    {
        if (storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text.IsNullOrEmpty())
            return;
        string buyItemName = storeBuyPanel.transform.GetChild(2).name;
        int buyItemCnt = int.Parse(storeBuyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text);
        int buyMoney = buyItemCnt * DataBase.Instance.itemInfoDict[buyItemName].buyPrice;
        if (buyItemCnt <= 0)
        {
            storeBuyPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = "0보다 큰 수를 입력해주세요.";
            storeBuyPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
            return;
        }
        if (buyItemCnt > DataBase.Instance.itemInfoDict[buyItemName].maxCarryAmount || buyItemCnt > 10000)
        {
            storeBuyPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text =
                string.Format("한번에 {0}개 이하만 구매 가능합니다.", DataBase.Instance.itemInfoDict[buyItemName].maxCarryAmount);
            storeBuyPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
            return;
        }
        if (buyMoney > DataBase.Instance.selectedCharacterSpec.money)
        {
            storeBuyPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text =
                string.Format("잔액이 부족합니다.\n{0}개 이하만 구매 가능합니다.",
                (int)(DataBase.Instance.selectedCharacterSpec.money / DataBase.Instance.itemInfoDict[buyItemName].buyPrice)
                );
            storeBuyPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
            return;
        }
        if (DataBase.Instance.myCharacterControl.getItem(new Item { itemName = buyItemName, itemCount = buyItemCnt }, false))
        {
            DataBase.Instance.selectedCharacterSpec.money -= buyMoney;
            UpdateStoreInventory();
            storeBuyPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
            storeBuyPanel.SetActive(false);
        }
        else
        {
            storeBuyPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = "인벤토리에 공간이 부족합니다.";
        }

    }
    #endregion

    #region 강화
    public void ShowEnchantPanel()
    {
        EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<itemslot>().itemName = "";
        UpdateEnchantPanel();
        EnchantResult.SetActive(false);
        isEnchanting = false;
        updateCurrentFocusWindow(EnchantPanel);
    }
    public void UpdateEnchantPanel()
    {
        EnchantPanel.GetComponent<Animator>().SetBool("enchant", false);
        itemslot slotInfo = EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<itemslot>();
        if (slotInfo.itemName.IsNullOrEmpty())
        {
            EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>().sprite = null;
            EnchantPanel.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = "";
            EnchantPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = "";
            EnchantPanel.transform.GetChild(2).GetChild(4).GetComponent<TMP_Text>().text = "";
            EnchantPanel.transform.GetChild(2).GetChild(5).gameObject.SetActive(false);
            EnchantPanel.transform.GetChild(2).GetChild(6).GetComponent<Button>().interactable = false;
            slotInfo.isBlank = true;
            enchantPercent = -1;
        }
        else
        {
            string iconDir = DataBase.Instance.itemInfoDict[slotInfo.itemName].iconDirectory;
            if (iconDir.IsNullOrEmpty())
                iconDir = DataBase.Instance.itemInfoDict[slotInfo.itemName].spriteDirectory;
            EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(iconDir);
            EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Image>().preserveAspect = true;
            int currentReinforce = 0;
            if (slotInfo.slotPos < 0)
            {
                currentReinforce = DataBase.Instance.selectedCharacterSpec.equipment[-slotInfo.slotPos - 1].reinforce;
            }
            else
            {
                currentReinforce = DataBase.Instance.selectedCharacterSpec.inventory[slotInfo.slotPos].reinforce;
            }
            enchantPercent = DataBase.Instance.CalEnchantPercent(currentReinforce);
            enchantPrice = DataBase.Instance.CalEnchantPrice(slotInfo.itemName, currentReinforce);
            EnchantPanel.transform.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = string.Format("{0} => {1}", currentReinforce, currentReinforce + 1);
            EnchantPanel.transform.GetChild(2).GetChild(2).name = currentReinforce.ToString();
            EnchantPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = enchantPercent.ToString() + "%";
            EnchantPanel.transform.GetChild(2).GetChild(4).GetComponent<TMP_Text>().text = "공격력 증가 +" + DataBase.Instance.CalEnchantPower(slotInfo.itemName, currentReinforce + 1).ToString();
            EnchantPanel.transform.GetChild(2).GetChild(5).gameObject.SetActive(true);
            EnchantPanel.transform.GetChild(2).GetChild(5).GetChild(1).GetComponent<TMP_Text>().text = enchantPrice[0].ToString();
            EnchantPanel.transform.GetChild(2).GetChild(5).GetChild(3).GetComponent<TMP_Text>().text = enchantPrice[1].ToString();
            EnchantPanel.transform.GetChild(2).GetChild(6).GetComponent<Button>().interactable = true;

        }
    }
    public void ClickEnchantButton()
    {
        //GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        //current_clicked_button.transform.parent.parent.GetComponent<Animator>().SetBool("enchant", true);
        EnchantPanel.transform.GetChild(2).GetChild(6).GetComponent<Button>().interactable = false;
        isEnchanting = true;
        itemslot slotInfo = EnchantPanel.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<itemslot>();
        StartCoroutine(enchantItem());
        int percent = UnityEngine.Random.Range(1, 101);
        int reinforce = int.Parse(EnchantPanel.transform.GetChild(2).GetChild(2).name);
        if (percent <= enchantPercent)
        {
            EnchantResult.transform.GetChild(0).GetComponent<TMP_Text>().text = "강화 성공";
            EnchantResult.transform.GetChild(1).GetComponent<TMP_Text>().text = string.Format("{0} -> {1}", reinforce, reinforce + 1);
            if (slotInfo.slotPos < 0)
            {
                DataBase.Instance.selectedCharacterSpec.equipment[-slotInfo.slotPos - 1].reinforce++;
            }
            else
            {
                DataBase.Instance.selectedCharacterSpec.inventory[slotInfo.slotPos].reinforce++;
            }
        }
        else
        {
            EnchantResult.transform.GetChild(0).GetComponent<TMP_Text>().text = "강화 실패";
            EnchantResult.transform.GetChild(1).GetComponent<TMP_Text>().text = string.Format("{0} -> {1}", reinforce, reinforce);
        }
        string iconDir = DataBase.Instance.itemInfoDict[slotInfo.itemName].iconDirectory;
        if (iconDir.IsNullOrEmpty())
            iconDir = DataBase.Instance.itemInfoDict[slotInfo.itemName].spriteDirectory;
        EnchantResult.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(iconDir);
        EnchantResult.transform.GetChild(2).GetChild(0).GetComponent<Image>().preserveAspect = true;
    }
    IEnumerator enchantItem()
    {
        EnchantPanel.GetComponent<Animator>().SetBool("enchant", true);
        float _time = 0f;
        while (_time < 1f)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        EnchantPanel.GetComponent<Animator>().SetBool("enchant", false);
        ShowEnchantResult();

    }
    void ShowEnchantResult()
    {
        EnchantResult.SetActive(true);
    }
    public void ClickEnchantResultButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.SetActive(false);
        isEnchanting = false;
        UpdateEnchantPanel();
    }
    #endregion

    #region 장비창
    public void UpdateEquipmentPanel()
    {
        for (int k = 0; k < 6; k++)
        {
            equipmentPanel.transform.GetChild(2).GetChild(k).GetChild(0).GetComponent<Image>().sprite = null;
            equipmentPanel.transform.GetChild(2).GetChild(k).GetChild(0).GetComponent<itemslot>().itemName = "";
            equipmentPanel.transform.GetChild(2).GetChild(k).GetChild(0).GetComponent<itemslot>().isBlank = true;
            equipmentPanel.transform.GetChild(2).GetChild(k).GetChild(1).gameObject.SetActive(true);
        }
        for (int k = 0; k < DataBase.Instance.selectedCharacterSpec.equipment.Count; k++) {
            InventoryItem item = DataBase.Instance.selectedCharacterSpec.equipment[k];
            string dir = DataBase.Instance.itemInfoDict[item.itemName].iconDirectory;
            string type = DataBase.Instance.itemInfoDict[item.itemName].itemType;
            if (DataBase.Instance.itemInfoDict[item.itemName].iconDirectory.IsNullOrEmpty())
                dir = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;
            int partsNum = -1;
            if (type == "helmet")
            {
                partsNum = 0;
            }
            else if (type == "cloth")
            {
                partsNum = 1;
            }
            else if (type == "armor")
            {
                partsNum = 2;
            }
            else if (type == "pant")
            {
                partsNum = 4;
            }
            else if (type == "back")
            {
                partsNum = 5;
            }
            else if (type.Contains("weapon"))
            {
                partsNum = 3;
            }
            if (partsNum == -1)
                continue;
            equipmentPanel.transform.GetChild(2).GetChild(partsNum).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(dir);
            equipmentPanel.transform.GetChild(2).GetChild(partsNum).GetChild(0).GetComponent<Image>().preserveAspect = true;
            equipmentPanel.transform.GetChild(2).GetChild(partsNum).GetChild(0).GetComponent<itemslot>().itemName = item.itemName;
            equipmentPanel.transform.GetChild(2).GetChild(partsNum).GetChild(0).GetComponent<itemslot>().isBlank = false;
            equipmentPanel.transform.GetChild(2).GetChild(partsNum).GetChild(0).GetComponent<itemslot>().slotPos = -(k + 1);
            equipmentPanel.transform.GetChild(2).GetChild(partsNum).GetChild(1).gameObject.SetActive(false);
        }
    }
    #endregion


    #region 교환창
    void UpdateTradePanel()
    {

    }


    void resetTradePanel()
    {
        resetMyTradePanel();
        resetOpTradePanel();
        tradeChatLog = "";
        tradeChatLogShow.text = tradeChatLog;
        tradeChatInput.text = "";        
        tradeCheckCntPanel.SetActive(false);
    }
    void resetMyTradePanel()
    {
        myTradeBox.transform.parent.GetChild(0).GetComponent<TMP_Text>().text = DataBase.Instance.myCharacterState.nick;
        myTradeBox.transform.parent.GetChild(4).gameObject.SetActive(false);
        for (int k = 0; k < 10; k++)
        {
            myTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<Image>().sprite = null;
            myTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<itemslot>().isBlank = true;
            myTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<itemslot>().itemName = "";
            myTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "";
            myTradeBox.transform.GetChild(k).GetChild(2).GetComponent<TMP_Text>().text = "";
            myTradeBox.transform.GetChild(k).GetChild(3).gameObject.SetActive(true);
        }
        tradePanel.transform.GetChild(2).GetChild(5).GetComponent<Button>().interactable = true;
    }
    void resetOpTradePanel()
    {
        opTradeBox.transform.parent.GetChild(0).GetComponent<TMP_Text>().text = inGameUserList[tradeOpName].nick;
        opTradeBox.transform.parent.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "요청 대기중..";
        opTradeBox.transform.parent.GetChild(4).gameObject.SetActive(true);        
        for (int k = 0; k < 10; k++)
        {
            opTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<Image>().sprite = null;
            opTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<itemslot>().isBlank = true;
            opTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "";
            opTradeBox.transform.GetChild(k).GetChild(2).GetComponent<TMP_Text>().text = "";
            opTradeBox.transform.GetChild(k).GetChild(3).gameObject.SetActive(true);
        }
        opAcceptTradeText.SetActive(false);
    }

    public void UpdateOpTradeItem(string itemName, int cnt, int slotPos, int enchant)
    {
        if (itemName == "money")
        {

        }
        else
        {
            itemslot updatingItemSlot = opTradeBox.transform.GetChild(slotPos).GetChild(1).GetChild(0).GetComponent<itemslot>();
            updatingItemSlot.itemName = itemName;
            updatingItemSlot.slotPos = enchant;
            updatingItemSlot.oriPos = cnt;
            updatingItemSlot.isBlank = false;
            if (DataBase.Instance.itemInfoDict[itemName].iconDirectory.IsNullOrEmpty())
                opTradeBox.transform.GetChild(slotPos).GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].spriteDirectory);
            else
                opTradeBox.transform.GetChild(slotPos).GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].iconDirectory);
            opTradeBox.transform.GetChild(slotPos).GetChild(2).GetComponent<TMP_Text>().text = itemName;
            opTradeBox.transform.GetChild(slotPos).GetChild(3).gameObject.SetActive(false);
            if (cnt == 0)
            {
                updatingItemSlot.transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            }
            else
            {
                updatingItemSlot.transform.GetChild(0).GetComponent<TMP_Text>().text = cnt.ToString();
            }
        }
    }
    public void UpdateMyTradeItem(string itemName, int cnt, int slotPos, int invenPos, int enchant)
    {
        if (itemName == "money")
        {

        }
        else
        {
            itemslot updatingItemSlot = myTradeBox.transform.GetChild(slotPos).GetChild(1).GetChild(0).GetComponent<itemslot>();
            updatingItemSlot.itemName = itemName;
            updatingItemSlot.transform.GetChild(0).name = cnt.ToString();
            updatingItemSlot.slotPos = enchant;
            updatingItemSlot.oriPos = invenPos;
            updatingItemSlot.isBlank = false;
            if (DataBase.Instance.itemInfoDict[itemName].iconDirectory.IsNullOrEmpty())
                myTradeBox.transform.GetChild(slotPos).GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].spriteDirectory);
            else
                myTradeBox.transform.GetChild(slotPos).GetChild(1).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].iconDirectory);
            myTradeBox.transform.GetChild(slotPos).GetChild(2).GetComponent<TMP_Text>().text = itemName;
            myTradeBox.transform.GetChild(slotPos).GetChild(3).gameObject.SetActive(false);
            if (cnt == 0)
            {
                updatingItemSlot.transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            }
            else
            {
                updatingItemSlot.transform.GetChild(0).GetComponent<TMP_Text>().text = cnt.ToString();
            }
        }
        newNetworkManager.Instance.PV.RPC("upTradeItem", inGameUserList[tradeOpName].PV.Owner, itemName, cnt, slotPos, enchant);
    }

    public void ClickSendTradeRequestButton()
    {
        string opName = userInteractionPanel.transform.GetChild(0).GetChild(1).name;
        if (!inGameUserList.ContainsKey(opName))
        {
            userInteractionPanel.SetActive(false);
            updateCurrentFocusWindow();
            return;
        }
        if (inGameUserList[opName].doingSomeThing)
        {
            popInfo("상대방이 교환 신청을 받을 수 없습니다.");
            userInteractionPanel.SetActive(false);
            updateCurrentFocusWindow();
            return;
        }
        tradeOpName = opName;
        DataBase.Instance.myCharacterState.updateDoing(true);
        newNetworkManager.Instance.PV.RPC("sendAndReceiveTradeRequest", inGameUserList[opName].PV.Owner, DataBase.Instance.myCharacter.name);
        userInteractionPanel.SetActive(false);
        updateCurrentFocusWindow();
        resetTradePanel();        
        updateCurrentFocusWindow(tradePanel);

    }

    public void receiveTradeRequest(string fromWho)
    {
        if (!inGameUserList.ContainsKey(fromWho))
        {
            UpdateInGameUser();
        }
        if (inGameUserList.ContainsKey(fromWho))
        {
            updateCurrentFocusWindow(tradeRequestPanel);
            tradeRequestPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = inGameUserList[fromWho].nick + "님이\n 교환 신청을 보냈습니다.";
            tradeOpName = fromWho;
        }
    }

    public void ClickTradeRequestButton(bool accept)
    {
        if (accept)
        {
            resetTradePanel();
            opTradeBox.transform.parent.GetChild(4).gameObject.SetActive(false);
            DataBase.Instance.myCharacterState.updateDoing(true);
            updateCurrentFocusWindow(tradePanel);
            newNetworkManager.Instance.PV.RPC("joinTradePanel", inGameUserList[tradeOpName].PV.Owner);
        }
        else
        {
            newNetworkManager.Instance.PV.RPC("leaveOrRejectTradePanel", inGameUserList[tradeOpName].PV.Owner, true);
        }
        tradeRequestPanel.SetActive(false);
        openedWindows.Remove(tradeRequestPanel);
        updateCurrentFocusWindow();
    }

    public void OpJoinTrade()
    {
        opTradeBox.transform.parent.GetChild(0).GetComponent<TMP_Text>().text = inGameUserList[tradeOpName].nick;
        opTradeBox.transform.parent.GetChild(4).gameObject.SetActive(false);        
        tradeChatLog += "\n" + inGameUserList[tradeOpName].nick + "님이 교환 신청을 수락하였습니다.";
        tradeChatLogShow.text = tradeChatLog;
    }

    public void OpLeaveTrade(bool reject)
    {
        tradePanel.SetActive(false);
        tradeRequestPanel.SetActive(false);
        openedWindows.Remove(tradePanel);
        openedWindows.Remove(tradeRequestPanel);
        updateCurrentFocusWindow();
        DataBase.Instance.myCharacterState.updateDoing(false);
        if (reject)
            popInfo("상대방이 교환을 거절하였습니다.");
        else
            popInfo("상대방이 교환을 취소하였습니다.");
        regetUpItems();
    }

    public void OpAcceptTrade()
    {
        opTradeBox.transform.parent.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "교환 수락";
        opTradeBox.transform.parent.GetChild(4).gameObject.SetActive(true);
        opAcceptTradeText.SetActive(true);        
    }

    public void ClickLeaveTradeButton()
    {
        if (!inGameUserList.ContainsKey(tradeOpName))
        {
            UpdateInGameUser();
        }
        if (!tradeOpName.IsNullOrEmpty() && inGameUserList.ContainsKey(tradeOpName))
            newNetworkManager.Instance.PV.RPC("leaveOrRejectTradePanel", inGameUserList[tradeOpName].PV.Owner, false);

        DataBase.Instance.myCharacterState.updateDoing(false);
        tradePanel.SetActive(false);
        openedWindows.Remove(tradePanel);
        updateCurrentFocusWindow();
        regetUpItems();
    }

    void regetUpItems()
    {
        for (int k = 0; k < 10; k++)
        {
            itemslot updatedItemSlot = myTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<itemslot>();
            if (updatedItemSlot.isBlank)
                continue;

            string itemName = updatedItemSlot.itemName;
            int cnt = int.Parse(myTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetChild(0).name);            
            int invenPos = updatedItemSlot.oriPos;
            int enchant = updatedItemSlot.slotPos;

            DataBase.Instance.myCharacterControl.getItem(new Item { reinforce = enchant, itemName = itemName, itemCount = cnt }, false, invenPos);
        }
    }

    void getTradeItems()
    {
        for (int k = 0; k < 10; k++)
        {
            itemslot updatedItemSlot = opTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<itemslot>();
            if (updatedItemSlot.isBlank)
                continue;

            string itemName = updatedItemSlot.itemName;
            int cnt = updatedItemSlot.oriPos;
            int enchant = updatedItemSlot.slotPos;
            
            DataBase.Instance.myCharacterControl.getItem(new Item { reinforce = enchant, itemName = itemName, itemCount = cnt }, false);
        }
    }


    void sendTradeChat()
    {
        if (tradeChatInput.text != "")
        {
            string chat = tradeChatInput.text;
            tradeChatLog += "\n" + PhotonNetwork.NickName + " : " + chat;
            tradeChatLogShow.text = tradeChatLog;

            newNetworkManager.Instance.PV.RPC("sendTradeChatLog", inGameUserList[opTradeBox.transform.parent.GetChild(0).name].PV.Owner, PhotonNetwork.NickName + " : " + chat);
            tradeChatInput.text = "";
            tradeChatEnd = false;
        }
        else
        {
            tradeChatInput.DeactivateInputField();
            tradeChatEnd = true;
        }
    }

    void popTradeCheckCnt(string itemName, int invenSlotPot, int slotPos)
    {
        tradeCheckCntPanel.transform.GetChild(2).name = itemName;
        tradeCheckCntPanel.transform.GetChild(2).GetChild(0).name = invenSlotPot.ToString();
        tradeCheckCntPanel.transform.GetChild(2).GetChild(1).name = slotPos.ToString();
        tradeCheckCntPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
        if (DataBase.Instance.itemInfoDict[itemName].itemType == "potion" || DataBase.Instance.itemInfoDict[itemName].itemType == "material")
        {
            tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = "";
            tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().interactable = true;
        }
        else
        {
            tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text = "1";
            tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().interactable = false;
        }
        tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().ActivateInputField();
        tradeCheckCntPanel.SetActive(true);
    }

    public void ClickTradeCheckCntButton()
    {
        if (tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text.IsNullOrEmpty())
            return;
        string upItemName = tradeCheckCntPanel.transform.GetChild(2).name;
        int upItemCnt = int.Parse(tradeCheckCntPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_InputField>().text);
        int invenPos = int.Parse(tradeCheckCntPanel.transform.GetChild(2).GetChild(0).name);
        int slotPos = int.Parse(tradeCheckCntPanel.transform.GetChild(2).GetChild(1).name);
        int enchant;
        if (upItemCnt <= 0)
        {
            tradeCheckCntPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text = "0보다 큰 수를 입력해주세요.";
            tradeCheckCntPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
            return;
        }
        if (DataBase.Instance.itemInfoDict[upItemName].itemType == "material" || DataBase.Instance.itemInfoDict[upItemName].itemType == "potion")
        {
            if (upItemCnt > quickInventory[upItemName].kindCount)
            {
                tradeCheckCntPanel.transform.GetChild(2).GetChild(3).GetComponent<TMP_Text>().text =
                    string.Format("아이템이 {0}개 보다 부족합니다.", upItemCnt);
                tradeCheckCntPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(true);
                return;
            }
            enchant = -1;
        }
        else
        {
            enchant = DataBase.Instance.selectedCharacterSpec.inventory[invenPos].reinforce;
        }

        DataBase.Instance.myCharacterControl.loseItem(upItemName, upItemCnt, invenPos);
        updateInventory();
        
        tradeCheckCntPanel.transform.GetChild(2).GetChild(3).gameObject.SetActive(false);
        tradeCheckCntPanel.SetActive(false);

        UpdateMyTradeItem(upItemName, upItemCnt, slotPos, invenPos, enchant);

    }

    public void TradeDone(int type)
    {
        if(type == 0)
        {
            popInfo("교환되었습니다.");
            getTradeItems();
        }
        else if( type == 1)
        {
            popInfo("교환에 실패했습니다. 상대방 인벤토리에 공간이 부족합니다.");
            regetUpItems();
        }
        else if (type == 2)
        {
            popInfo("교환에 실패했습니다. 인벤토리에 공간이 부족합니다.");
            regetUpItems();
        }
        else
        {
            popInfo("알 수 없는 이유로 교환에 실패하였습니다.");
            regetUpItems();
        }        
        DataBase.Instance.myCharacterState.updateDoing(false);
        tradePanel.SetActive(false);
        openedWindows.Remove(tradePanel);
        updateCurrentFocusWindow();
    }


    public void ClickTradeAcceptButton()
    {
        tradePanel.transform.GetChild(2).GetChild(5).GetComponent<Button>().interactable = false;
        tradeCheckCntPanel.SetActive(false);
        myTradeBox.transform.parent.GetChild(4).gameObject.SetActive(true);
        newNetworkManager.Instance.PV.RPC("acceptTrade", inGameUserList[tradeOpName].PV.Owner);
        if (opAcceptTradeText.activeSelf)
        {
            bool iCan = CheckTradable();
            newNetworkManager.Instance.PV.RPC("tryTrade", inGameUserList[tradeOpName].PV.Owner, iCan);
        }
    }
    bool CheckTradable()
    {
        int blank_slot_cnt = 0;
        Dictionary<string, int> opUpItem = new Dictionary<string, int>();
        for (int k = 0; k < DataBase.Instance.selectedCharacterSpec.inventory.Count; k++)
        {
            if (DataBase.Instance.selectedCharacterSpec.inventory[k] == null)
            {
                blank_slot_cnt++;
            }
        }
        for (int k = 0; k < opTradeBox.transform.childCount; k++)
        {
            itemslot upItem = opTradeBox.transform.GetChild(k).GetChild(1).GetChild(0).GetComponent<itemslot>();
            if (!upItem.isBlank)
            {
                if (!opUpItem.ContainsKey(upItem.itemName))
                    opUpItem.Add(upItem.itemName, upItem.oriPos);
                else
                    opUpItem[upItem.itemName] += upItem.oriPos;
            }
        }
        foreach (string itemName in opUpItem.Keys)
        {
            int cnt = opUpItem[itemName];
            int slotCnt;
            
            if(quickInventory.ContainsKey(itemName))
            {
                int already_slot = quickInventory[itemName].position.Count;
                int need_slot = (quickInventory[itemName].kindCount + cnt) / DataBase.Instance.itemInfoDict[itemName].maxCarryAmount;
                if ((quickInventory[itemName].kindCount + cnt) % DataBase.Instance.itemInfoDict[itemName].maxCarryAmount != 0)
                    need_slot++;
                if (already_slot >= need_slot)
                    slotCnt = 0;
                else
                    slotCnt = need_slot - already_slot;
            }
            else
            {
                if (cnt > DataBase.Instance.itemInfoDict[itemName].maxCarryAmount)
                {
                    slotCnt = cnt / DataBase.Instance.itemInfoDict[itemName].maxCarryAmount;
                    if (cnt % DataBase.Instance.itemInfoDict[itemName].maxCarryAmount != 0)
                        slotCnt++;
                }
                else
                {
                    slotCnt = 1;
                }
            }
            blank_slot_cnt -= slotCnt;
            if (blank_slot_cnt < 0)
                return false;
        }
        return true;
    }
    
    public void tryTrade(bool opCan)
    {
        bool iCan = CheckTradable();

        if (opCan && iCan)
        {
            TradeDone(0);
            newNetworkManager.Instance.PV.RPC("tradeDone", inGameUserList[tradeOpName].PV.Owner, 0);
        }
        else if (opCan && !iCan)
        {
            TradeDone(2);
            newNetworkManager.Instance.PV.RPC("tradeDone", inGameUserList[tradeOpName].PV.Owner, 1);
        }
        else if (!opCan && iCan)
        {
            newNetworkManager.Instance.PV.RPC("tradeDone", inGameUserList[tradeOpName].PV.Owner, 2);
            TradeDone(1);
        }
        else if (!opCan && !iCan)
        {
            newNetworkManager.Instance.PV.RPC("tradeDone", inGameUserList[tradeOpName].PV.Owner, 2);
            TradeDone(2);
        }
        else
        {
            newNetworkManager.Instance.PV.RPC("tradeDone", inGameUserList[tradeOpName].PV.Owner, 3);
            TradeDone(3);
        }
    }
    #endregion

    #region 옵션

    public void popInfo(string content)
    {
        GameObject newInfo = Instantiate(infoPopUpPanel);
        newInfo.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = content;
        newInfo.transform.parent = transform;
        newInfo.transform.localScale = Vector3.one;
        newInfo.transform.localPosition = Vector3.zero;
        newInfo.GetComponent<Canvas>().sortingOrder = openedWindows.Count + 6;
        newInfo.SetActive(true);
    }
    public void ClickClosePop()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        Destroy(current_clicked_button.transform.parent.gameObject);
    }
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
        newNetworkManager.Instance.ClickDisconnectButton();
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

