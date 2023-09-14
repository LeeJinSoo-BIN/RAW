using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEyeAttack3 : MonoBehaviour
{
    // Start is called before the first frame update
    private float Damage = 70f;
    // Start is called before the first frame update

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.transform.GetComponent<PhotonView>().IsMine)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, Damage);
            }
        }
    }
}
