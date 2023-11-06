using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterDirectAttack : MonoBehaviour
{   
    public bool dealOnce = true;
    public int Deal;
    public bool foot = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Player") && ((foot == true && collision.name == "foot") || (foot == false && collision.name != "foot")) && dealOnce)
        {
            if (collision.transform.GetComponent<PhotonView>().IsMine)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, Deal);
                dealOnce = false;
            }
        }
    }
}
