using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portal : MonoBehaviour
{
    newNetworkManager networkManager;    
    private void Awake()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<newNetworkManager>();
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (collision.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                if (DataBase.Instance.isCaptain)
                {
                    UIManager.Instance.EnterDungeonPop();
                }
            }
        }
    }
}
