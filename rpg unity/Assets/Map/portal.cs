using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portal : MonoBehaviour
{
    newNetworkManager networkManager;
    UIManager uiManager;
    private void Awake()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<newNetworkManager>();
        uiManager = GameObject.Find("Panel Canvas").GetComponent<UIManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (collision.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                if (networkManager.myPartyCaptainName == DataBase.Instance.currentCharacterNickname)
                {
                    uiManager.EnterDungeonPop();
                }
            }
        }
    }
}
