using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_InputField pwInputField;
    public TMP_InputField pwCheckInputField;
    public TMP_InputField idInputField;
    private Selectable currentField;
    void Start()
    {
        
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
        
       
    }

    public void Show_Hide_PassWordCheck(bool show)
    {
        if (pwCheckInputField.gameObject.activeSelf == show)
            return;
        if (show)
        {
            idInputField.transform.position = new Vector3(idInputField.transform.position.x, idInputField.transform.position.y + 50, 0);
            pwInputField.transform.position = new Vector3(pwInputField.transform.position.x, pwInputField.transform.position.y + 50, 0);
            pwCheckInputField.gameObject.SetActive(show);
        }
        else
        {
            idInputField.transform.position = new Vector3(idInputField.transform.position.x, idInputField.transform.position.y - 50, 0);
            pwInputField.transform.position = new Vector3(pwInputField.transform.position.x, pwInputField.transform.position.y - 50, 0);
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
            Debug.Log("login");
        }
    }

    public void ClickChannel()
    {
        string channelName = EventSystem.current.currentSelectedGameObject.name;
        PhotonNetwork.JoinRoom(channelName);
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
}
