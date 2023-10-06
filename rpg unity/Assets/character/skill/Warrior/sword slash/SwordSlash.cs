using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SwordSlash : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;
    private float DealSync;
    private bool IsCritical;
    public PolygonCollider2D attackArea;
    //public GameObject target;
    //public Vector2 targetPos;
    public PhotonView PV;
    
    IEnumerator Excute()
    {
        float _time = 0f;
        while(_time < DealSync)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        attackArea.enabled = true;
    }

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
            GetComponentInChildren<Animator>().SetTrigger("vanish");            
        }
        catch { }
        Destroy(gameObject, 0.45f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Monster") && PV.IsMine && collision.name != "foot")
        {
            PhotonView MonsterPV = collision.transform.GetComponent<PhotonView>();
            MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f, IsCritical);
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, bool isCritical, float sync, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        //Power = power;
        IsCritical = isCritical;
        if (target_name != "")
        {
            //target = GameObject.Find(target_name);
            //transform.parent = target.transform;
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }
        DealSync = sync;
        StartCoroutine(Vanish(duration));
        StartCoroutine(Excute());
    }

}
