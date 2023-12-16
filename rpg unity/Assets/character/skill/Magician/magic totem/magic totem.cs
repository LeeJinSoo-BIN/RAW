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
    private bool IsCritical;
    //public GameObject target;
    //public Vector2 targetPos;
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
        if (PV.IsMine)
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
        if (PV.IsMine)
            PhotonNetwork.Destroy(PV);        
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
        if (collision == null)
            return;
        if (collision.CompareTag("Monster") && collision.name == "foot" && PV.IsMine)
        {
            if (_time <= dropTime)
            {
                PhotonView MonsterPV = collision.transform.parent.GetComponent<PhotonView>();
                MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f, IsCritical);                
            }
        }
        else if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (collision.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                GameObject new_aura = Instantiate(aura);
                new_aura.transform.parent = collision.transform.parent;
                new_aura.transform.localPosition = Vector3.zero;
                new_aura.transform.localScale = Vector3.one;
                new_aura.name = "aura";
                CharacterState state = collision.transform.parent.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(3, Power);
            }
        }        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null)
            return;
        if (collision.CompareTag("Player") && collision.name =="foot")            
        {
            if (collision.transform.parent.GetComponent<PhotonView>().IsMine)
            {
                Transform having_aura = collision.transform.parent.Find("aura");
                if (having_aura != null)
                {
                    having_aura.GetChild(0).GetComponent<Animator>().SetTrigger("vanish");
                    Destroy(having_aura.gameObject, 0.45f);
                }
                CharacterState state = collision.transform.parent.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(3, -Power);
            }
        }
    }

    [PunRPC]
    void initSkill(float deal, float heal, float sheild, float power, bool isCritical, float sync, float duration, string target_name, Vector2 target_pos)
    {
        Deal = deal;
        //Heal = heal;
        //Shield = sheild;
        Power = power;
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
        StartCoroutine(Vanish(duration));
        StartCoroutine(Excute());
    }
}
