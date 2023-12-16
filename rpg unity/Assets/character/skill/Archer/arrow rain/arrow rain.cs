using UnityEngine;
using System.Collections;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;

public class ArrowRain : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;
    private bool IsCritical;
    private float dealSync;
    //public GameObject target;
    public Vector2 targetPos;
    public PhotonView PV;

    IEnumerator Excute()
    {
        float _timer = 0f;
        while(_timer < dealSync)
        {
            _timer += Time.deltaTime;
            yield return null;
        }
        GetComponent<PolygonCollider2D>().enabled = true;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Monster") && collision.name == "foot" && PV.IsMine)
        {
            PhotonView MonsterPV = collision.transform.parent.GetComponent<PhotonView>();
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
            targetPos = target_pos;
        }
        dealSync = sync;
        StartCoroutine(Excute());
        StartCoroutine(Vanish(duration));
    }
}
