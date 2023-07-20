using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMagent : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            collision.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            collision.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
