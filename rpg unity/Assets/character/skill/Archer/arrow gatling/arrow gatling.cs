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
    private bool IsCritical;
    //public GameObject target;
    private Vector2 targetPos;
    bool isMine = false;

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
                    //PV.RPC("destroySelf", RpcTarget.AllBuffered, 0.3f);
                    Destroy(gameObject, 0.3f);
                }
                else
                {
                    //PV.RPC("destroySelf", RpcTarget.AllBuffered, 0f);
                    Destroy(gameObject);
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
        if (collision.CompareTag("Monster") && collision.name != "foot")
        {
            targetPos = transform.position;
            explosion = true;
            if (isMine)
            {
                PhotonView MonsterPV = collision.transform.GetComponent<PhotonView>();
                MonsterPV.RPC("MonsterDamage", RpcTarget.All, 0, Deal, 0f, IsCritical);
            }
        }
    }
    public void initSkill(float deal, bool isCritical, Vector2 target_pos, bool _isMine)
    {
        Deal = deal;
        IsCritical = isCritical;
        isMine = _isMine;
        targetPos = target_pos;
        StartCoroutine(Excute());
    }
}


