using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject currentFocusWindow;
    public GameObject inventoryPanel;
    public GameObject optionPanel;
    private HashSet<GameObject> openedWindows = new HashSet<GameObject>();
    void Start()
    {
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
            Debug.Log("I");
            Debug.Log(inventoryPanel.activeSelf);
            if(inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(false);
                openedWindows.Remove(inventoryPanel);
                updateCurrentFocusWindow();
            }
            else
            {
                updateCurrentFocusWindow(inventoryPanel);
            }
        }
            
    }

    void updateCurrentFocusWindow(GameObject currentWindow = null)
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
}
