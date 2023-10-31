using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEyeAttack3 : MonoBehaviour
{    
    private float Damage = 70f;
    private bool dealOnce = true;    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Player") && collision.name != "foot" && dealOnce)
        {
            if (collision.transform.GetComponent<PhotonView>().IsMine)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, Damage);
                dealOnce = false;
            }
        }
    }
}
