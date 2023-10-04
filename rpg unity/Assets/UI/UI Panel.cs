using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    Vector3 distanceMosePos = new Vector3(0f, 0f, 0f);
    private bool draging = false;
    private void Update()
    {
        if(draging)
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(currentMousePos.x + distanceMosePos.x, currentMousePos.y + distanceMosePos.y);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedPanel = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        if (clickedObject.name == "back" || clickedObject.name == "drag area")
        {
            UIManager.Instance.currentFocusWindow.GetComponent<Canvas>().sortingOrder -= 1;
            clickedPanel.GetComponent<Canvas>().sortingOrder = UIManager.Instance.openedWindows.Count + 5;
            UIManager.Instance.currentFocusWindow = clickedPanel;
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
    public void CloseButtonClick()
    {
        GameObject current_clicked_button = EventSystem.current.currentSelectedGameObject;
        current_clicked_button.transform.parent.gameObject.SetActive(false);
        UIManager.Instance.openedWindows.Remove(current_clicked_button.transform.parent.gameObject);
        UIManager.Instance.updateCurrentFocusWindow();
    }
    
}