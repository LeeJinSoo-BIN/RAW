using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EvilWizardSkill3 : MonoBehaviourPunCallbacks
{
    float Deal;
    private void Start()
    {
        Deal = int.Parse(name);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision == null)
            return;
        if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (collision.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                CharacterState state = collision.transform.parent.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, Deal);
            }
        }
    }
}
