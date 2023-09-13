using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ArrowNormal : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;    
    public GameObject target;
    public Vector2 targetPos;
    public PhotonView PV;

    private float speed = 5.5f;
    private bool explosion = false;
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
                    Destroy(gameObject, 0.3f);
                }
                else
                {
                    Destroy(gameObject);
                    break;
                }
            }
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);


            float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
            float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
            /*if (transform.localScale.x > 0)
                angle_rad -= 180;*/
            transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);
            yield return null;
        }
    }

    void destroy_self()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
    void initSkill(float deal, float heal, float sheild, float power, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        //Power = power
        if (target_name != "")
        {
            target = GameObject.Find(target_name);            
            targetPos = target.transform.Find("hit position").transform.position;
        }
        if (target_pos != default(Vector2))
        {
            //targetPos = target_pos;
        }

        StartCoroutine(Excute());
    }
}

