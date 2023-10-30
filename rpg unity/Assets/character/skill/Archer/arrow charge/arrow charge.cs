using UnityEngine;
using System.Collections;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;

public class ArrowCharge : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;
    private bool IsCritical;
    //public GameObject target;
    public Vector2 targetPos;
    public PhotonView PV;

    private float speed = 7f;
    private float charge_time = 2.5f;
    private float current_time = 0f;

    
    IEnumerator Excute()
    {
        while (true)
        {
            if (charge_time > current_time)
            {
                current_time += Time.deltaTime;
                float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
                float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
                transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);
            }
            else
            {
                Vector2 dir = targetPos - (Vector2)transform.position;
                if (dir.magnitude < 0.01f)
                {
                    if(PV.IsMine)
                        PV.RPC("destroySelf", RpcTarget.AllBuffered);
                    break;
                }

                transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

                float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
                float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
                transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);
            }
            yield return null;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (current_time > charge_time)
        {
            if (collision.CompareTag("Monster") && PV.IsMine && collision.name != "foot")
            {
                PhotonView MonsterPV = collision.transform.GetComponent<PhotonView>();
                MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f, IsCritical);                
            }
        }
    }

    [PunRPC]
    void destroySelf()
    {
        try
        {
            GetComponent<Animator>().SetTrigger("vanish");
        }
        catch { }
        Destroy(gameObject);
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
        StartCoroutine(Excute());
    }
}

