using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ArrowDash : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject target;
    private Vector2 targetPos;
    public PhotonView PV;
    IEnumerator Excute(Vector2 goalPos, float speed)
    {
        target.GetComponent<MultyPlayer>().goalPos = targetPos;
        Vector3 _dirVec = goalPos - (Vector2)target.transform.position;
        target.GetComponent<PhotonView>().RPC("direction", RpcTarget.All, _dirVec);
        while (true)
        {
            _dirVec = goalPos - (Vector2)target.transform.position;
            if (_dirVec.sqrMagnitude < 0.001f)
            {
                target.transform.position = goalPos;
                break;
            }
            target.transform.position += _dirVec.normalized * speed * Time.deltaTime;
            yield return null;
        }
        PV.RPC("destroySelf", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void destroySelf()
    {
        /*try
        {
            GetComponent<Animator>().SetTrigger("vanish");
        }
        catch { }*/
        Destroy(gameObject);
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float sync, float duration, string target_name, Vector2 target_pos)
    {
        //Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        //Power = power;
        if (target_name != "")
        {
            target = GameObject.Find(target_name);
            transform.parent = target.transform;
        }
        if (target_pos != default(Vector2))
        {
            targetPos = target_pos -= new Vector2(0f, 0.3f);
        }
        StartCoroutine(Excute(target_pos, 5f));
    }    
}
