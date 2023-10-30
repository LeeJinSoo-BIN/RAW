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
    private float Sync;
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
        if(PV.IsMine)
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

    IEnumerator gainShield()
    {
        if (!PV.IsMine)
            yield break;
        float _timer = 0f;
        while(_timer < Sync)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        target.transform.GetComponentInChildren<CharacterState>().ProcessSkill(2, Shield);
    }

    void aggro(string target_name, float duration)
    {
        foreach(Transform monster in GameObject.Find("Enemy Group").transform)
        {
            monster.GetComponent<MonsterControl>().PV.RPC("aggro", RpcTarget.All, target_name, duration);
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, bool isCritical, float sync, float duration, string target_name, Vector2 target_pos)
    {
        //Deal = deal;
        //Heal = heal;
        Shield = sheild;
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
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }
        Sync = sync;
        StartCoroutine(Vanish(sync));
        StartCoroutine(gainShield());
        if(PV.IsMine)
            aggro(target_name, duration);
    }

}
