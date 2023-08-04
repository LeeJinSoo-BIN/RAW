using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMagnet : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("??");
        Debug.Log(collision.name);
        if (collision.gameObject.tag == "Item")
        {
            collision.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("???");
        if (collision.gameObject.tag == "Item")
        {
            collision.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
