using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SwordShield : MonoBehaviourPunCallbacks
{
    //private float Deal;
    //private float Heal;
    private float Shield;
    //private float Power;
    public GameObject target;
    //public Vector2 targetPos;
    public PhotonView PV;

    IEnumerator Vanish(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        GetComponent<Animator>().SetTrigger("vanish");
        Destroy(gameObject, 0.45f);
    }

    public void gainShield()
    {
        if (PV.IsMine)
        {
            CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(2, Shield);
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float duration, string target_name, Vector2 target_pos)
    {
        //Deal = deal;
        //Heal = heal;
        Shield = sheild;
        //Power = power;
        if (target_name != "")
        {
            target = GameObject.Find(target_name);
            transform.parent = target.transform;
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }
        StartCoroutine(Vanish(duration));

        gainShield();
    }

}
