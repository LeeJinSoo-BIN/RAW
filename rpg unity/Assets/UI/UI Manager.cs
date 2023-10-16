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

public class UIManager : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    //public static UIManager Instance;
    public GameObject currentFocusWindow;
    public GameObject inventoryPanel;
    public GameObject skillPanel;
    public GameObject optionPanel;
    public GameObject partyPanel;
    public GameObject invitePartyPanel;
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();    
    public GameObject myCharacter;    
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    public GameObject skillBox;
    public GameObject skillInfo;

    public GameObject inGameUserBox;
    public GameObject inGameUserInfo;
    private Dictionary<string, Player> inGameUserList = new Dictionary<string, Player>();

    public GameObject partyMemberBox;
    public GameObject partyMemberInfo;

    public TMP_InputField chatInput;

    public GameObject PlayerGroup;
    public newNetworkManager networkManager;
    public bool oldVersion = false;
    void Awake()
    {
        //Instance = this;
        inventoryPanel.SetActive(false);
        optionPanel.SetActive(false);
        skillPanel.SetActive(false);
        if (!oldVersion)
        {
            partyPanel.SetActive(false);
            inGameUserInfo.SetActive(false);
            invitePartyPanel.SetActive(false);
        }
        chatInput = GameObject.Find("In Game UI Canvas").transform.Find("Game").Find("Chat").Find("chat Input").GetComponent<TMP_InputField>();
        //skillBox = skillPanel.transform.GetChild(2).gameObject;
        //skillInfo = skillPanel.transform.GetChild(3).gameObject;
    }
    void Start()
    {
        if (!oldVersion)
            UpdatePartyPanel();
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
            Debug.Log("Esc");
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
        if (!chatInput.isFocused)
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


        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                currentFocusWindow = null;
            }
        }
    }
    public void SetUP()
    {
        ResetSkillPanel();
        UpdateSkillPanel();
    }

    public void setUI()
    {

    }
    public void ClickSkillLevelUpButton()
    {

    }    
    public void updateCurrentFocusWindow(GameObject currentWindow = null)
    {        
        if(currentWindow != null)
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

    public void ResetSkillPanel()
    {
        for(int k = 0; k < skillBox.transform.childCount; k++)
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
            if(skillBox.transform.Find(name) == null)
            {
                GameObject newSkill = Instantiate(skillInfo);
                newSkill.name = name;                
                newSkill.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(Path.Combine(DataBase.Instance.skillThumbnailPath, name));
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

    public void ClickPartyInviteButton()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        Debug.Log(inGameUserList[current_clicked_button.name]);
        networkManager.InviteParty(inGameUserList[current_clicked_button.name], "초대초대");
    }
    
    public void UpdatePartyPanel()
    {
        if (!PhotonNetwork.InRoom)
            return;
        UpdatePartyMember();
        UpdateInGameUser();
    }
    
    public void UpdatePartyMember()
    {
        
    }
    public void UpdateInGameUser()
    {
        for (int k = 0; k < inGameUserBox.transform.childCount; k++)
        {
            Destroy(inGameUserBox.transform.GetChild(k).gameObject);
        }
        inGameUserList.Clear();
        foreach (Transform user in PlayerGroup.transform)
        {
            if (user.GetComponent<PhotonView>().IsMine)
            {
                continue;
            }
            else
            {
                CharacterState currentUserState = user.GetComponent<CharacterState>();
                GameObject userInfo = Instantiate(inGameUserInfo);
                userInfo.transform.GetChild(0).GetComponent<TMP_Text>().text = "닉네임: " + currentUserState.nick;
                userInfo.transform.GetChild(1).GetComponent<TMP_Text>().text = "Lv. " + currentUserState.level.ToString();
                userInfo.transform.GetChild(2).GetComponent<TMP_Text>().text = "직업: " + currentUserState.roll;
                userInfo.transform.GetChild(3).name = currentUserState.nick;
                inGameUserList.Add(currentUserState.nick, currentUserState.PV.Owner);
                userInfo.transform.parent = inGameUserBox.transform;
                userInfo.transform.localScale = Vector3.one;
                userInfo.SetActive(true);
            }
        }

    }
    public void recieveInvite(string ment)
    {
        invitePartyPanel.SetActive(true);
        invitePartyPanel.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = ment;
    }
    public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        updateCurrentFocusWindow();
    }
}
