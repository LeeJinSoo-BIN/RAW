using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    //public static UIManager Instance;
    public GameObject currentFocusWindow;
    public GameObject inventoryPanel;
    public GameObject skillPanel;
    public GameObject optionPanel;
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();    
    public GameObject myCharacter;    
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    public GameObject skillBox;
    public GameObject skillInfo;

    public TMP_InputField chatInput;
    void Awake()
    {
        //Instance = this;
        inventoryPanel.SetActive(false);
        optionPanel.SetActive(false);
        skillBox = skillPanel.transform.GetChild(2).gameObject;
        skillInfo = skillPanel.transform.GetChild(3).gameObject;
    }
    void Start()
    {
        chatInput = GameObject.Find("In Game UI Canvas").transform.Find("Game").Find("Chat").Find("chat Input").GetComponent<TMP_InputField>();
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
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (inventoryPanel.activeSelf)
                {
                    if (currentFocusWindow == inventoryPanel)
                    {
                        inventoryPanel.SetActive(false);
                        openedWindows.Remove(inventoryPanel);
                        updateCurrentFocusWindow();
                    }
                    else
                    {
                        currentFocusWindow.GetComponent<Canvas>().sortingOrder -= 1;
                        inventoryPanel.GetComponent<Canvas>().sortingOrder = openedWindows.Count + 5;
                        currentFocusWindow = inventoryPanel;
                    }
                }
                else
                {
                    updateCurrentFocusWindow(inventoryPanel);
                }
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                if (skillPanel.activeSelf)
                {
                    if (currentFocusWindow == skillPanel)
                    {
                        skillPanel.SetActive(false);
                        openedWindows.Remove(skillPanel);
                        updateCurrentFocusWindow();
                    }
                    else
                    {
                        currentFocusWindow.GetComponent<Canvas>().sortingOrder -= 1;
                        skillPanel.GetComponent<Canvas>().sortingOrder = openedWindows.Count + 5;
                        currentFocusWindow = skillPanel;
                    }
                }
                else
                {
                    updateCurrentFocusWindow(skillPanel);
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

    
    public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        updateCurrentFocusWindow();
    }
}
