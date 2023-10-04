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
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
        if (clickedObject.name == "back")
        {
            UIManager.Instance.currentFocusWindow.GetComponent<Canvas>().sortingOrder -= 1;
            clickedObject.GetComponent<Canvas>().sortingOrder = UIManager.Instance.openedWindows.Count + 5;
            UIManager.Instance.currentFocusWindow = clickedObject;
            if (eventData.pointerCurrentRaycast.gameObject.name == "drag area")
            {
                Vector3 dragStartMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                distanceMosePos.x = clickedObject.transform.position.x - dragStartMousePos.x;
                distanceMosePos.y = clickedObject.transform.position.y - dragStartMousePos.y;
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
