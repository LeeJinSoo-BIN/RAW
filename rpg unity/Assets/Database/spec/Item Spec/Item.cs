using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{    
    public string itemName;
    public int itemCount;
    //public ItemSpec spec;
    public PhotonView PV;
    private GameObject ItemField;

    private void Awake()
    {
        ItemField = GameObject.Find("Item Field");
    }
    [PunRPC]
    void initItem(string item_name, int cnt)
    {
        itemName = item_name;
        Sprite sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[itemName].spriteDirectory);
        //spec = DataBase.Instance.itemInfoDict[itemName];
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sprite;
        transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = sprite;
        transform.parent = ItemField.transform;
        name = item_name + ItemField.transform.childCount;
        itemCount = cnt;
    }
}

