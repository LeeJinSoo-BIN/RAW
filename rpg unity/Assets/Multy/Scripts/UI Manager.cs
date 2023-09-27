using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject currentFocusWindow;
    public GameObject inventoryPanel;
    public GameObject optionPanel;
    private HashSet<GameObject> openedWindows;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentFocusWindow.activeSelf)
            {
                currentFocusWindow.SetActive(false);                
            }
            else
            {
                optionPanel.SetActive(true);
                currentFocusWindow = optionPanel;
            }
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            if(inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(false);
            }
            else
            {
                inventoryPanel.SetActive(true);
                openedWindows.Add(inventoryPanel);
                currentFocusWindow = inventoryPanel;
            }
        }
            
    }
}
