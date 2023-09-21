using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ArrowGatlingShooter : MonoBehaviour
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    //private float Power;    
    //public GameObject target;
    private Vector2 targetPos;
    public PhotonView PV;

    IEnumerator Excute(Vector2 desPos)
    {
        for (int k = 0; k < 20; k++)
        {
            float _time = 0;
            while (true)
            {
                if (_time > 0.05f)
                {
                    break;
                }
                _time += Time.deltaTime;
                yield return null;
            }
            float rand_x = Random.Range(-0.1f, 0.1f);
            float rand_y = Random.Range(-0.1f, 0.1f);
            GameObject skill = PhotonNetwork.Instantiate("Character/skills/arrow gatling arrow", (Vector2)transform.position + new Vector2(rand_x, rand_y), Quaternion.identity);
            skill.GetComponent<PhotonView>().RPC("initSkill", RpcTarget.AllBuffered, Deal, 0f, 0f, 0f, 1f, "", desPos + new Vector2(rand_x, rand_y));
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
        StartCoroutine(Excute(target_pos));
    }
}
