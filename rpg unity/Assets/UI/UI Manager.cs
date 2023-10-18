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
    public GameObject joinPartyRequestPanel;
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();    
    public GameObject myCharacter;    
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    public GameObject skillBox;
    public GameObject skillInfo;

    public GameObject inGameUserPanel;
    public GameObject inGameUserBox;
    public GameObject inGameUserInfo;
    public Dictionary<string, Player> inGameUserList = new Dictionary<string, Player>();
    public Dictionary<int, string> idToNickName = new Dictionary<int, string>();
    public GameObject partyMemberBox;
    public GameObject partyMemberInfo;
    public TMP_InputField partyMakeNameInput;

    public GameObject partyListPanel;
    public GameObject partyListBox;
    public GameObject partyListInfo;


    public GameObject enterDungeonPanel;
    public TMP_InputField timeLimitInputfield;

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
            invitePartyPanel.SetActive(false);
            joinPartyRequestPanel.SetActive(false);

            inGameUserInfo.SetActive(false);
            partyMemberInfo.SetActive(false);
            partyListInfo.SetActive(false);
            enterDungeonPanel.SetActive(false);
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
                        if(currentFocusWindow != null)
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

    
    public void UpdatePartyPanel()
    {
        if (!PhotonNetwork.InRoom)
            return;
        UpdatePartyMember();        
        UpdateInGameUser();
        UpdatePartyList();
        /*if(networkManager.myPartyCaptainName == DataBase.Instance.currentCharacterNickname)
        {
            inGameUserPanel.SetActive(true);
        }
        else
        {
            inGameUserPanel.SetActive(false);
        }*/


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
                Debug.Log(memberNickName);
                GameObject newMember = Instantiate(partyMemberInfo);
                newMember.transform.GetChild(1).GetComponent<TMP_Text>().text = PlayerGroup.transform.Find(memberNickName).GetComponent<CharacterState>().nick;
                if (memberNickName == networkManager.myPartyCaptainName)
                    newMember.transform.GetChild(1).GetComponent<TMP_Text>().text = "*" + newMember.transform.GetChild(1).GetComponent<TMP_Text>().text;
                newMember.transform.GetChild(2).GetComponent<TMP_Text>().text = "Lv. " + PlayerGroup.transform.Find(memberNickName).GetComponent<CharacterState>().level.ToString();
                newMember.transform.GetChild(3).GetComponent<TMP_Text>().text = "직업: " + PlayerGroup.transform.Find(memberNickName).GetComponent<CharacterState>().roll;
                newMember.transform.GetChild(4).name = memberNickName;
                if (networkManager.myPartyCaptainName != DataBase.Instance.currentCharacterNickname)
                    newMember.transform.GetChild(4).GetComponent<Button>().interactable = false;
                else if (memberNickName == DataBase.Instance.currentCharacterNickname)
                    newMember.transform.GetChild(4).GetComponent<Button>().interactable= false;
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
    }

    public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        updateCurrentFocusWindow();
    }
}
