using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static UIManager Instance;
    public GameObject currentFocusWindow;
    public GameObject inventoryPanel;
    public GameObject skillPanel;
    public GameObject optionPanel;
    public HashSet<GameObject> openedWindows = new HashSet<GameObject>();
    void Start()
    {
        Instance = this;
        inventoryPanel.SetActive(false);
        optionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc");
            if (openedWindows.Count > 0)                
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
        else if (Input.GetKeyDown(KeyCode.I))
        {
            if(inventoryPanel.activeSelf)
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
            foreach(GameObject window in openedWindows)
            {
                if (window.GetComponent<Canvas>().sortingOrder == openedWindows.Count + 5)
                    currentFocusWindow = window;
            }
        }
        Debug.Log(openedWindows.Count + " " + currentFocusWindow);
    }
    /*public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        updateCurrentFocusWindow();
    }*/

}
