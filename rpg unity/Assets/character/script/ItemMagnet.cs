using Photon.Pun;
using UnityEngine;

public class ItemMagnet : MonoBehaviour
{
    public PhotonView PV;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {        
        if (collision.gameObject.tag == "Item" && PV.IsMine)
        {
            collision.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item" && PV.IsMine)
        {
            collision.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
