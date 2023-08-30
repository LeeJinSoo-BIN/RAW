using UnityEngine;
using System.Collections;
using Photon.Pun;
using static UnityEngine.GraphicsBuffer;

public class MagicTotem : MonoBehaviourPunCallbacks
{
    private float Deal;
    //private float Heal;
    //private float Shield;
    private float Power;    
    //public GameObject target;
    //public Vector3 targetPos;
    public PhotonView PV;

    float dropTime = 0.3f;
    float _time = 0f;
    public GameObject aura;

    IEnumerator Vanish(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        GetComponent<Animator>().SetTrigger("vanish");
        Destroy(gameObject, 0.45f);
    }
    
    IEnumerator Excute()
    {
        while(_time <= dropTime)
        {
            _time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster") && PV.IsMine)
        {
            if (_time <= dropTime)
            {
                PhotonView MonsterPV = collision.transform.GetComponent<PhotonView>();
                MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f);                
            }
        }
        else if (collision.CompareTag("Player") && collision.transform.GetComponent<PhotonView>().IsMine)
        {
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(3, Power);
        }        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.transform.GetComponent<PhotonView>().IsMine)
        {
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(3, -Power);
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, float duration, string target_name, Vector3 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        Power = power;
        if (target_name != "")
        {
            //target = GameObject.Find(target_name);
            //transform.parent = target.transform;
        }
        if (target_pos != default(Vector3))
        {
            //targetPos = target_pos;
        }
        StartCoroutine(Vanish(duration));

        StartCoroutine(Excute());
    }
}
