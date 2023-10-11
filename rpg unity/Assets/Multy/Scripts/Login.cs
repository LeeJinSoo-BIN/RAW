using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

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
    public AccountInfo defaultAccountInfo;    

    public GameObject LoginPanel;
    public GameObject SelectCharacterPanel;
    void Start()
    {
        LoginPanel.SetActive(true);
        SelectCharacterPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (pwInputField.isFocused || pwCheckInputField.isFocused) // password 에 포커스가 되어있을때
        {
            Input.imeCompositionMode = IMECompositionMode.Off;
        }
        else if (idInputField.isFocused)
        {            
            //Input.imeCompositionMode = IMECompositionMode.On;
        }
        else
            Input.imeCompositionMode = IMECompositionMode.Auto;

        /*if (PhotonNetwork.IsConnected)
        {
            updatePlayerCount();
        }*/
    }

    public void Show_Hide_PassWordCheck(bool show)
    {
        if (pwCheckInputField.gameObject.activeSelf == show)
            return;
        if (show)
        {
            idInputField.transform.localPosition = new Vector3(idInputField.transform.localPosition.x, idInputField.transform.localPosition.y + 90, 0);
            pwInputField.transform.localPosition = new Vector3(pwInputField.transform.localPosition.x, pwInputField.transform.localPosition.y + 90, 0);
            pwCheckInputField.gameObject.SetActive(show);
        }
        else
        {
            idInputField.transform.localPosition = new Vector3(idInputField.transform.localPosition.x, idInputField.transform.localPosition.y - 90, 0);
            pwInputField.transform.localPosition = new Vector3(pwInputField.transform.localPosition.x, pwInputField.transform.localPosition.y - 90, 0);
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

    public void LogIn()
    {
        if (!pwCheckInputField.gameObject.activeSelf)
        {
            if (!idInputField.text.IsNullOrEmpty() && !pwInputField.text.IsNullOrEmpty())
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
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

        foreach (CharacterSpec character in defaultAccountInfo.characterList)
        {
            GameObject characterButton = Instantiate(characterSelectButton);
            List<InventoryItem> equipment = character.equipment;
            SPUM_SpriteList spriteList = characterButton.transform.GetChild(0).GetComponentInChildren<SPUM_SpriteList>();
            foreach (InventoryItem item in equipment)
            {
                string current_item_sprite = DataBase.Instance.itemInfoDict[item.itemName].spriteDirectory;
                spriteList.PartsPath[DataBase.Instance.itemInfoDict[item.itemName].itemType] = current_item_sprite;
                //Debug.Log(spriteList.PartsPath[itemInfoDict[item.itemName].itemType]);
            }
            spriteList._hairAndEyeColor = character.colors;
            spriteList.setSprite();

            characterButton.transform.GetChild(1).name = character.nickName;
            characterButton.transform.GetChild(2).GetComponent<TMP_Text>().text = character.nickName;

            characterButton.SetActive(true);
            characterButton.transform.parent = characterSelectList.transform;
            characterButton.transform.localPosition = Vector3.zero;
            characterButton.transform.localScale = Vector3.one;
        }
    }

    public override void OnJoinedRoom()
    {

    }
        public void ClickCharacterSelectButton()
    {
        string channelName = EventSystem.current.currentSelectedGameObject.name;
        PhotonNetwork.JoinOrCreateRoom("palletTown", new RoomOptions { MaxPlayers = maxNumServerPlayer }, null);
    }
}
