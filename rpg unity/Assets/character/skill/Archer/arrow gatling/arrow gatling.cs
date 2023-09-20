using UnityEngine;
using System.Collections;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;

public class ArrowGatling : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;    
    //public GameObject target;
    private Vector2 targetPos;
    public PhotonView PV;

    private float speed = 8f;
    private bool explosion = false;
    private bool isRotated = false;


    IEnumerator Excute()
    {
        while (true)
        {
            Vector2 dir = targetPos - (Vector2)transform.position;
            if (dir.magnitude < 0.01f)
            {
                if (explosion)
                {
                    transform.GetChild(1).gameObject.SetActive(true);
                    transform.GetChild(0).gameObject.SetActive(false);
                    PV.RPC("destroySelf", RpcTarget.AllBuffered);
                }
                else
                {
                    PV.RPC("destroySelf", RpcTarget.AllBuffered);
                    break;
                }
            }
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            if (!isRotated)
            {
                float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
                float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
                /*if (transform.localScale.x > 0)
                    angle_rad -= 180;*/
                transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);
                isRotated = true;
            }
            yield return null;
        }
    }    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Monster"))
        {
            targetPos = transform.position;
            explosion = true;
            if (PV.IsMine)
            {
                PhotonView MonsterPV = collision.transform.GetComponent<PhotonView>();
                MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f);
            }
        }

    }
    [PunRPC]
    void destroySelf()
    {
        Destroy(gameObject, 0.45f);
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        //Power = power;
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


