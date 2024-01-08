using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portal : MonoBehaviour
{
    //newNetworkManager networkManager;
    int playerOn = 0;
    public GameObject portalHole;
    public Collider2D portalPlate;
    public GameManager gameManager;
    private void Awake()
    {
        //networkManager = GameObject.Find("NetworkManager").GetComponent<newNetworkManager>();
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;

        if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (DataBase.Instance.currentMapType == "village")
            {
                if (collision.transform.parent.GetComponent<PhotonView>().IsMine)
                {
                    if (DataBase.Instance.isCaptain)
                    {
                        UIManager.Instance.EnterDungeonPop();
                    }
                }
            }
            else if(DataBase.Instance.currentMapType == "dungeon")
            {
                playerOn++;
                if(playerOn == DataBase.Instance.myPartyMemNum)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if(DataBase.Instance.currentStage == DataBase.Instance.dungeonInfoDict[DataBase.Instance.currentMapName].monsterInfoList.Count)
                        {
                            UIManager.Instance.popGameClearPanel();
                        }
                        else
                        {
                            newNetworkManager.Instance.PV.RPC("NextStage", RpcTarget.All);
                        }                        
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;
        if (DataBase.Instance.currentMapType == "dungeon")
        {
            if (collision.CompareTag("Player") && collision.name == "foot")
            {
                playerOn--;
            }
        }
    }

    public void ActivatePortal(bool onOff)
    {
        portalHole.SetActive(onOff);
        portalPlate.enabled = onOff;
    }
}
