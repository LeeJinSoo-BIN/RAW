using UnityEngine;
using System.Collections;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;

public class MagicFloor : MonoBehaviourPunCallbacks
{
    private float Deal;
    private float Heal;
    //private float Shield;
    //private float Power;
    //public GameObject target;
    //public Vector2 targetPos;
    public PhotonView PV;

    private float time = 0f;
    private float cycle = 1f;
    private bool active = true;    

    IEnumerator Vanish(float Duration)
    {
        float time = 0;
        while (time < Duration)
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

    void Update()
    {
        if (time > cycle)
        {
            active = true;
            time = 0;
        }
        else
        {
            active = false;
            time += Time.deltaTime;
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Monster") && collision.name == "foot" && PV.IsMine)
        {
            if (active)
            {
                PhotonView MonsterPV = collision.transform.parent.GetComponent<PhotonView>();
                MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f);
            }
        }
        else if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (active && collision.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                CharacterState state = collision.transform.parent.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(1, Heal);
            }
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float sync, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        Heal = heal;
        //Shield = sheild;
        //Power = power;
        if (target_name != "")
        {
            //target = GameObject.Find(target_name);
            //transform.parent = target.transform;
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }
        StartCoroutine(Vanish(duration));
    }
}
