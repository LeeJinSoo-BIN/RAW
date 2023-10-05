using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SwordBind : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
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
        PV.RPC("destroySelf", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void destroySelf()
    {
        try
        {
            GetComponent<Animator>().SetTrigger("vanish");
        }
        catch { }
        Destroy(gameObject, 0.45f);
    }
    public void Bind(float duration)
    {
        if (PV.IsMine)
        {
            PhotonView MonsterPV = target.transform.GetComponent<PhotonView>();
            MonsterPV.RPC("MonsterDamage", RpcTarget.All, 4, Deal, duration);
        }
    }


    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float sync, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        //Power = power;
        if (target_name != "")
        {
            target = GameObject.Find(target_name);
            if (target == null)
            {
                PV.RPC("destroySelf", RpcTarget.AllBuffered, 0f);
                return;
            }
            transform.parent = target.transform;
            transform.localPosition = new Vector3(-0.02f, 0.2f);
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }
        StartCoroutine(Vanish(duration));

        Bind(duration);
    }
}
