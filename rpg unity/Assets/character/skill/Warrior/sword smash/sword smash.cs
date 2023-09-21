using UnityEngine;
using System.Collections;
using Photon.Pun;

public class SwordSmash : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;    
    public GameObject target;
    //public Vector3 targetPos;
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
    public void giveDeal()
    {
        if (PV.IsMine)
        {
            PhotonView MonsterPV = target.transform.GetComponent<PhotonView>();
            MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f);
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        //Power = power
        if(target_name != "")
        {
            target = GameObject.Find(target_name);
            transform.parent = target.transform;
        }
        if(target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }        
        StartCoroutine(Vanish(duration));
        if(transform.localScale.x < 0f)
            transform.position += new Vector3(-1.5f, 0.3f);
        else
            transform.position += new Vector3(1.5f, 0.3f);
        giveDeal();
    }
}
