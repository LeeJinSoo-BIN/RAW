using UnityEngine;
using System.Collections;
using Photon.Pun;

public class MagicGlobalHeal : MonoBehaviourPunCallbacks
{
    //private float Deal;
    private float Heal;
    //private float Shield;
    //private float Power;
    private float DealSync;
    public GameObject target;
    //public Vector2 targetPos;
    public PhotonView parentPV;
    public PhotonView PV;
    IEnumerator Excute()
    {
        if (!parentPV.IsMine)
            yield break;
        float _timer = 0f;
        while (_timer < DealSync)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        transform.parent.GetComponentInChildren<CharacterState>().ProcessSkill(1, Heal);
    }

    IEnumerator Vanish(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        if (PV.IsMine)
            PhotonNetwork.Destroy(PV);
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


    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, bool isCritical, float sync, float duration, string target_name, Vector2 target_pos)
    {
        //Deal = deal;
        Heal = heal;
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
            transform.localPosition = Vector3.zero;
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }
        parentPV = transform.parent.GetComponent<PhotonView>();
        DealSync = sync;
        StartCoroutine(Vanish(duration));
        StartCoroutine(Excute());
        
    }
}
