using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class arrowcharge : MonoBehaviourPunCallbacks
{
    public Vector3 targetPos;
    public float speed = 1;
    private float charge_time = 2.5f;
    private float current_time = 0f;

    private int flatDeal = 1;
    private int dealIncreasePerSkillLevel = 1;
    private int dealIncreasePerPower = 1;
    public float caseterPower = 1f;
    public float casterSkillLevel = 1f;
    public float casterCriticalPercent = 1f;
    public float casterCriticalDamage = 1f;
    public float Deal;

    public PhotonView PV;

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
        if (charge_time > current_time)
        {
            current_time += Time.deltaTime;
            float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
            float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
            /*if (transform.localScale.x > 0)
                angle_rad -= 180;*/
            transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);
        }
        else
        {
            Vector3 dir = targetPos - transform.position;
            if (dir.magnitude < 0.01f)
            {
                Destroy(gameObject);
            }

            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            //transform.position += dir * speed * Time.deltaTime;


            float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
            float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
            /*if (transform.localScale.x > 0)
                angle_rad -= 180;*/
            transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);
        }
    }

    void destroy_self()
    {
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (current_time > charge_time)
        {
            if (collision.CompareTag("Monster") && PV.IsMine)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, Deal);
            }
        }
    }

    [PunRPC]
    void SetTargetPosition(Vector2 pos)
    {
        targetPos = pos;
    }
}

