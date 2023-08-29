using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SwordSmash : MonoBehaviourPunCallbacks
{
    private float flatDeal = 1;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;
    public GameObject target;

    private float duration = 0.7f;
    public float caseterPower = 1f;
    public int casterSkillLevel = 1;
    public float casterCriticalPercent = 1f;
    public float casterCriticalDamage = 1f;
    public float Deal;

    public PhotonView PV;

    private void Awake()
    {
        Deal = SkillManager.instance.CaculateCharacterSkillDamage(casterSkillLevel, caseterPower,
            flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower,
            casterCriticalPercent, casterCriticalDamage, true);
        StartCoroutine(Vanish(duration));
    }

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

    public void giveDeal()
    {
        if (PV.IsMine)
        {
            PhotonView PV = target.transform.GetComponent<PhotonView>();
            PV.RPC("MonsterDamage", RpcTarget.All, 0, Deal);
        }
    }

    [PunRPC]
    void initSkill(float power, int skillLevel, float criticalPercent, float criticalDamage)
    {
        caseterPower = power;
        casterSkillLevel = skillLevel;
        casterCriticalPercent = criticalPercent;
        casterCriticalDamage = criticalDamage;
    }

    [PunRPC]
    void SetTarget(string targetName)
    {
        target = GameObject.Find(targetName);
        transform.parent = target.transform;
        transform.position += new Vector3(1.5f, 0.3f);
        giveDeal();
    }
}
