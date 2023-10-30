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

public class UIManager : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{
    [Header("Panel")]
    #region
    public GameObject currentFocusWindow;
    public GameObject inventoryPanel;
    public GameObject enterDungeonPanel;
    public GameObject gameOverPanel;
    public TMP_InputField timeLimitInputfield;
    public GameObject toolTipPanel;
    public GameObject conversationPanel;

    [Header("Option Panel")]
    public GameObject optionPanel;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Text windowText;

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

    [Header("Data ")]
    #region
    public static UIManager Instance;
    public GameObject PlayerGroup;
    public GameObject EnemyGroup;
    public newNetworkManager networkManager;
    public GameObject myCharacter;
    public GameObject Boss;
    private CharacterState myCharacterState;
    private MonsterState bossState;
    private bool isBossConnected;
    public float limitTime;
    public float stageTime;
    private IEnumerator timer;

    public Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    private List<string> quickSlotKeys = new List<string> { "1", "2", "3", "4" };
    public Dictionary<string, string> keyToItemName = new Dictionary<string, string>();
    public Dictionary<string, qucikInventoryInfo> quickInventory;
    public Dictionary<string, Player> inGameUserList = new Dictionary<string, Player>();
    public Dictionary<int, string> idToNickName = new Dictionary<int, string>();
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();
    private Color failColor = new Color((94f / 255f), 0, 0);
    private Color succesColor = new Color(0, (94f / 255f), 0);
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    private bool chatEnd = false;
    public string chatLog;

    public float stayTime = 1f;
    private bool isHoverToolTip = false;
    private GameObject hoverObject;
    private float hoverTime = 0f;


    public LayerMask npcLayer;
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
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit_npc = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, npcLayer);
                if (hit_npc.collider != null)
                {
                    if (hit_npc.collider.CompareTag("NPC"))
                    {
                        ShowConversationPanel(hit_npc.transform.gameObject);
                    }
                }
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
        chatLogShow .text = chatLog;
    }


    public void SetUP()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<newNetworkManager>();
        PlayerGroup = GameObject.Find("Player Group");
        EnemyGroup = GameObject.Find("Enemy Group");
        GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GetComponent<Canvas>().sortingLayerName = "ui";


        ResetSkillPanel();
        UpdateSkillPanel();
        UpdatePartyPanel();

        myCharacterState = myCharacter.GetComponentInChildren<CharacterState>();
        makeProfile();
        characterHealth = myCharacterState.health;
        characterMana = myCharacterState.mana;
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
        if (limitTime <= 0)
            limitTime = 6000;
        timer = startTimer();
        StartCoroutine(timer);
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

    IEnumerator startTimer()
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
        GameObject myCharacterHead = Instantiate(myCharacter.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
        makeNewHead(myCharacterHead);

        foreach (Transform child in myCharacterProfileUiGroup.transform.GetChild(0))
            Destroy(child.gameObject);
        myCharacterHead.transform.parent = myCharacterProfileUiGroup.transform.GetChild(0).transform;
        myCharacterHead.transform.localPosition = new Vector3(0f, 1f, 0f);
        myCharacterProfileUiGroup.transform.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + myCharacterState.characterSpec.characterLevel.ToString();
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
        if (quickInventory.ContainsKey(keyToItemName[key]))
        {
            if (quickInventory[keyToItemName[key]].count > 0)
            {
                quickInventory[keyToItemName[key]].count--;
            }
            consumePotion(keyToItemName[key]);
            myCharacter.GetComponent<MultyPlayer>().updateInventory();
            updateThisQuickSlot(key);
        }
        /*
        for (int k = 0; k < inventory.Count; k++)
        {
            if(inventory[k].itemName == keyToItemName[key])
            {
                if (inventory[k].count > 0)
                {
                    inventory[k].count--;
                    consumePotion(keyToItemName[key]);
                    myCharacter.GetComponent<MultyPlayer>().updateInventory();
                    updateThisQuickSlot(key);
                }
            }
        }*/
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
                currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[keyToItemName[quickSlotKeys[k]]].count.ToString();
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
            currentSlot.GetChild(2).GetComponent<TMP_Text>().text = quickInventory[keyToItemName[key]].count.ToString();
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
        myCharacterState.ProcessSkill(1, DataBase.Instance.itemInfoDict[itemName].recoveryHealth);
        myCharacterState.ProcessSkill(5, DataBase.Instance.itemInfoDict[itemName].recoveryMana);
    }


    public void ShowConversationPanel(GameObject NPC)
    {        
        updateCurrentFocusWindow(conversationPanel);
        GameObject npcHead = Instantiate(NPC.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
        makeNewHead(npcHead);

        npcHead.transform.parent = conversationPanel.transform.GetChild(2).GetChild(0);
        npcHead.transform.localPosition = new Vector3(0, -30, 0);
        npcHead.transform.localScale = new Vector3(120, 120);

    }

    public void ClickExpandChatLog()
    {        
        if (ChatBox.sizeDelta.y == 120)
            ChatBox.sizeDelta = new Vector2(ChatBox.sizeDelta.x, 500);
        else
            ChatBox.sizeDelta = new Vector2(ChatBox.sizeDelta.x, 120);
        ChatExpandButtonIcon.localScale = new Vector3(ChatExpandButtonIcon.localScale.x, -ChatExpandButtonIcon.localScale.y, 1);
    }
    public void ClickSkillLevelUpButton()
    {

    }
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



    public void EnterToolTip()
    {
        Debug.Log("In");
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
        Debug.Log("out");
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
                DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerSkillLevel * myCharacterState.characterSpec.skillLevel[toolTipName] +
                DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerPower * myCharacterState.power).ToString());

            toolTipContent = toolTipContent.Replace("(flatDeal)", DataBase.Instance.skillInfoDict[toolTipName].flatDeal.ToString());
            toolTipContent = toolTipContent.Replace("(dealIncreasePerSkillLevel)", (DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerSkillLevel * myCharacterState.characterSpec.skillLevel[toolTipName]).ToString());
            toolTipContent = toolTipContent.Replace("(dealIncreasePerPower)", (DataBase.Instance.skillInfoDict[toolTipName].dealIncreasePerPower * myCharacterState.power).ToString());

            toolTipContent = toolTipContent.Replace("(flatHeal)", DataBase.Instance.skillInfoDict[toolTipName].flatHeal.ToString());
            toolTipContent = toolTipContent.Replace("(healIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(healIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].healIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(flatShield)", DataBase.Instance.skillInfoDict[toolTipName].flatShield.ToString());
            toolTipContent = toolTipContent.Replace("(shieldIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(shieldIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].shieldIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(flatPower)", DataBase.Instance.skillInfoDict[toolTipName].flatPower.ToString());
            toolTipContent = toolTipContent.Replace("(powerIncreasePerSkillLevel)", DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerSkillLevel.ToString());
            toolTipContent = toolTipContent.Replace("(powerIncreasePerPower)", DataBase.Instance.skillInfoDict[toolTipName].powerIncreasePerPower.ToString());

            toolTipContent = toolTipContent.Replace("(coolDown)", DataBase.Instance.skillInfoDict[toolTipName].coolDown.ToString());
            toolTipContent = toolTipContent.Replace("(consumeMana)", DataBase.Instance.skillInfoDict[toolTipName].consumeMana.ToString());

        }
        else if (hoverObject.name.Contains("item"))
        {
            toolTipName = hoverObject.name.Substring(5);
            toolTipContent = DataBase.Instance.itemInfoDict[toolTipName].description;
        }
        else
            return;
        
        toolTipPanel.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = toolTipName;
        toolTipPanel.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = toolTipContent;
        toolTipPanel.transform.position = hoverObject.transform.position;
        toolTipPanel.SetActive(true);
    }
    public void ResetSkillPanel()
    {
        for (int k = 0; k < skillBox.transform.childCount; k++)
        {
            Destroy(skillBox.transform.GetChild(k).gameObject);
        }
    }
    public void UpdateSkillPanel()
    {
        List<string> skillName = myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel.SD_Keys;
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
                string current_level = myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel[name].ToString();
                newSkill.transform.GetChild(3).GetComponent<TMP_Text>().text = current_level + " / " + max_level;
                newSkill.transform.SetParent(skillBox.transform, false);
                newSkill.transform.localPosition = Vector3.zero;
                newSkill.transform.localScale = Vector3.one;
                newSkill.gameObject.SetActive(true);
            }
            else
            {
                string max_level = DataBase.Instance.skillInfoDict[name].maxLevel.ToString();
                string current_level = myCharacter.GetComponent<MultyPlayer>().characterState.characterSpec.skillLevel[name].ToString();
                skillBox.transform.Find(name).transform.GetChild(3).GetComponent<TMP_Text>().text = current_level + " / " + max_level;
            }
        }
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

    #region 파티
    public void UpdatePartyPanel()
    {
        if (!PhotonNetwork.InRoom)
            return;
        UpdatePartyMember();        
        UpdateInGameUser();
        UpdatePartyList();
    }
    
    public void UpdatePartyMember()
    {
        for (int k = 0; k < partyMemberBox.transform.childCount; k++)
        {
            Destroy(partyMemberBox.transform.GetChild(k).gameObject);
        }
        if (!networkManager.myPartyCaptainName.IsNullOrEmpty())
        {
            foreach (string memberNickName in networkManager.allPartys[networkManager.myPartyCaptainName].partyMembersNickName)
            {
                GameObject member = PlayerGroup.transform.Find(memberNickName).gameObject;
                GameObject memberHead = Instantiate(member.transform.Find("Root").GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject);
                makeNewHead(memberHead);
                GameObject newMember = Instantiate(partyMemberInfo);
                memberHead.transform.parent = newMember.transform.GetChild(1);
                memberHead.transform.localPosition = new Vector3(0, -30, 0);
                memberHead.transform.localScale = new Vector3(120, 120);
                newMember.transform.GetChild(2).GetComponent<TMP_Text>().text = member.GetComponent<CharacterState>().nick;
                if (memberNickName == networkManager.myPartyCaptainName)
                    newMember.transform.GetChild(2).GetComponent<TMP_Text>().text = "*" + newMember.transform.GetChild(2).GetComponent<TMP_Text>().text;
                newMember.transform.GetChild(3).GetComponent<TMP_Text>().text = "Lv. " + member.GetComponent<CharacterState>().level.ToString();
                newMember.transform.GetChild(4).GetComponent<TMP_Text>().text = "직업: " + member.GetComponent<CharacterState>().roll;
                newMember.transform.GetChild(5).name = memberNickName;
                if (networkManager.myPartyCaptainName != DataBase.Instance.currentCharacterNickname)
                    newMember.transform.GetChild(5).GetComponent<Button>().interactable = false;
                else if (memberNickName == DataBase.Instance.currentCharacterNickname)
                    newMember.transform.GetChild(5).GetComponent<Button>().interactable= false;
                newMember.transform.parent = partyMemberBox.transform;
                
                newMember.SetActive(true);
                newMember.transform.localScale = Vector3.one;
            }        
        }
    }
    public void UpdateInGameUser()
    {
        for (int k = 0; k < inGameUserBox.transform.childCount; k++)
        {
            Destroy(inGameUserBox.transform.GetChild(k).gameObject);
        }
        inGameUserList.Clear();
        idToNickName.Clear();
        foreach (Transform user in PlayerGroup.transform)
        {
            if (user.GetComponent<PhotonView>().IsMine)
            {
                continue;
            }
            else
            {
                CharacterState currentUserState = user.GetComponent<CharacterState>();
                inGameUserList.Add(user.name, currentUserState.PV.Owner);
                idToNickName.Add(currentUserState.PV.Owner.ActorNumber, user.name);
                if (networkManager.usersInParty.Contains(user.name))
                    continue;

                GameObject userInfo = Instantiate(inGameUserInfo);
                userInfo.transform.GetChild(0).GetComponent<TMP_Text>().text = "닉네임: " + currentUserState.nick;
                userInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + currentUserState.level.ToString();
                userInfo.transform.GetChild(2).GetComponent<TMP_Text>().text = "직업: " + currentUserState.roll;
                userInfo.transform.GetChild(3).name = user.name;
                if (networkManager.myPartyCaptainName != DataBase.Instance.currentCharacterNickname)
                    userInfo.transform.GetChild(3).GetComponent<Button>().interactable = false;
                else if (networkManager.allPartys[DataBase.Instance.currentCharacterNickname].partyMembersNickName.Count >= 3)
                    userInfo.transform.GetChild(3).GetComponent<Button>().interactable = false;

                userInfo.transform.parent = inGameUserBox.transform;
                userInfo.transform.localScale = Vector3.one;
                userInfo.SetActive(true);
            }
        }

    }
    void UpdatePartyList()
    {
        for (int k = 0; k < partyListBox.transform.childCount; k++)
        {
            Destroy(partyListBox.transform.GetChild(k).gameObject);
        }
        foreach (string captain in networkManager.allPartys.Keys)
        {
            GameObject newParty = Instantiate(partyListInfo);
            string partyName = networkManager.allPartys[captain].partyName;
            int currentMemNum = networkManager.allPartys[captain].partyMembersNickName.Count;

            newParty.transform.GetChild(0).GetComponent<TMP_Text>().text = "파티장: " + PlayerGroup.transform.Find(captain).GetComponent<CharacterState>().nick;
            newParty.transform.GetChild(1).GetComponent<TMP_Text>().text = "파티명: " + partyName;
            newParty.transform.GetChild(2).GetComponent<TMP_Text>().text = "인원: " + currentMemNum.ToString() + "/3";
            newParty.transform.GetChild(3).name = captain;
            if (networkManager.usersInParty.Contains(DataBase.Instance.currentCharacterNickname) || currentMemNum == 3)
            {
                newParty.transform.GetChild(3).GetComponent<Button>().interactable = false;
            }

            newParty.transform.parent = partyListBox.transform;
            newParty.SetActive(true);
            newParty.transform.localScale = Vector3.one;
        }
    }


    public void ClickMakePartyButton()
    {
        if (networkManager.usersInParty.Contains(DataBase.Instance.currentCharacterNickname))
            return;
        string partyName = partyMakeNameInput.text;
        if (partyMakeNameInput.text.IsNullOrEmpty())
            partyName = "파티 고고";
        networkManager.myPartyCaptainName = DataBase.Instance.currentCharacterNickname;
        networkManager.PV.RPC("registParty", RpcTarget.AllBuffered, partyName, DataBase.Instance.currentCharacterNickname);
        
    }


    public void ClickLeavePartyButton()
    {
        if (networkManager.myPartyCaptainName.IsNullOrEmpty())
            return;
        if(networkManager.myPartyCaptainName == DataBase.Instance.currentCharacterNickname)
        {
            if (networkManager.allPartys[networkManager.myPartyCaptainName].partyMembersNickName.Count == 1)
            {
                networkManager.PV.RPC("BoomParty", RpcTarget.AllBuffered, DataBase.Instance.currentCharacterNickname);
            }
            else
            {
                foreach(string memName in networkManager.allPartys[DataBase.Instance.currentCharacterNickname].partyMembersNickName)
                {
                    if (memName == DataBase.Instance.currentCharacterNickname)
                        continue;
                    networkManager.PV.RPC("ChangeCaptain", RpcTarget.AllBuffered, DataBase.Instance.currentCharacterNickname, memName, true);
                    break;
                }
            }
        }
        else
        {
            networkManager.PV.RPC("LeaveParty", RpcTarget.AllBuffered, networkManager.myPartyCaptainName, DataBase.Instance.currentCharacterNickname);
        }
    }

    public void ClickKickPartyMemberButton()
    {
        if (networkManager.myPartyCaptainName != DataBase.Instance.currentCharacterNickname)
            return;
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        networkManager.PV.RPC("kickPartyMember", RpcTarget.AllBuffered, DataBase.Instance.currentCharacterNickname, current_clicked_button.name);
    }

    public void ClickAcceptPartyInviteButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        if (networkManager.allPartys[current_clicked_button.name].partyMembersNickName.Count < 3)
        {
            networkManager.PV.RPC("joinParty", RpcTarget.AllBuffered, current_clicked_button.name, DataBase.Instance.currentCharacterNickname);
        }
        else
        {

        }
        invitePartyPanel.SetActive(false);
        openedWindows.Remove(invitePartyPanel);
        updateCurrentFocusWindow();
    }

    public void ClickRejectPartyInviteButton()
    {
        invitePartyPanel.SetActive(false);
        openedWindows.Remove(invitePartyPanel);
        updateCurrentFocusWindow();
    }

    public void ClickAcceptJoinPartyRequestButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        if (networkManager.allPartys[DataBase.Instance.currentCharacterNickname].partyMembersNickName.Count < 3)
        {
            networkManager.PV.RPC("joinParty", RpcTarget.AllBuffered, DataBase.Instance.currentCharacterNickname, current_clicked_button.name);
        }
        else
        {

        }
        joinPartyRequestPanel.SetActive(false);        
        openedWindows.Remove(joinPartyRequestPanel);
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
        Debug.Log(inGameUserList[current_clicked_button.name]);
        networkManager.PV.RPC("sendAndReceiveInviteParty",
            inGameUserList[current_clicked_button.name],
            networkManager.allPartys[networkManager.myPartyCaptainName].partyName,
            DataBase.Instance.currentCharacterNickname);
    }

    public void ClickPartyJoinRequsetButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        networkManager.PV.RPC("sendAndReceiveJoinRequestParty", inGameUserList[current_clicked_button.name], DataBase.Instance.currentCharacterNickname);
    }
    public void receiveInvite(string partyName, string captain)
    {        
        updateCurrentFocusWindow(invitePartyPanel);
        CharacterState captainInfo = PlayerGroup.transform.Find(captain).GetComponent<CharacterState>();
        string captainNick = captainInfo.nick;
        string captainLevel = captainInfo.level.ToString();
        string captainRoll = captainInfo.roll;
        invitePartyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = string.Format("{0}님\r\n레벨: {1}\r\n직업: {2}\r\n이 파티 초대를 보냈습니다.\r\n\r\n파티명: {3}", captainNick, captainLevel, captainRoll, partyName);
        invitePartyPanel.transform.GetChild(2).GetChild(1).name = captain;
    }
    public void receiveJoinRequest(string fromWho)
    {
        updateCurrentFocusWindow(joinPartyRequestPanel);
        CharacterState captainInfo = PlayerGroup.transform.Find(fromWho).GetComponent<CharacterState>();
        string captainNick = captainInfo.nick;
        string captainLevel = captainInfo.level.ToString();
        string captainRoll = captainInfo.roll;
        joinPartyRequestPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = string.Format("{0}님\r\n레벨: {1}\r\n직업: {2}\r\n이 파티 가입 요청을 보냈습니다.", captainNick, captainLevel, captainRoll);
        joinPartyRequestPanel.transform.GetChild(2).GetChild(1).name = fromWho;
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

    public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        updateCurrentFocusWindow();
    }
}
