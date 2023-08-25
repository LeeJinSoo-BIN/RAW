using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;

public class arrowgatling : MonoBehaviourPunCallbacks
{
    public Vector3 targetPos;
    public float speed = 1;
    private bool explosion = false;
    private bool isRotated = false;

    private float flatDeal = 1;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;
    public float caseterPower = 1f;
    public float casterSkillLevel = 1f;
    public float casterCriticalPercent = 1f;
    public float casterCriticalDamage = 1f;
    public float Deal;
    // Start is called before the first frame update
    void Start()
    {
        speed = 10;
        Deal = SkillManager.instance.CaculateCharacterSkillDamage(casterSkillLevel, caseterPower,
            flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower,
            casterCriticalPercent, casterCriticalDamage, true);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = targetPos - transform.position;
        if (dir.magnitude < 0.01f)
        {
            if (explosion)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(0).gameObject.SetActive(false);
                Invoke("destroy_self", 0.3f);
            }
            else
            {
                Destroy(gameObject);
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
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(0, Deal);
        }

    }

    [PunRPC]
    void SetTargetPosition(Vector2 pos)
    {
        targetPos = pos;
    }
}


