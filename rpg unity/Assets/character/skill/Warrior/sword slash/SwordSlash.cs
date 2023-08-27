using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private float flatDeal = 15;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;

    private float duration = 1.2f;
    public float caseterPower = 1f;
    public float casterSkillLevel = 1f;
    public float casterCriticalPercent = 1f;
    public float casterCriticalDamage = 1f;
    public float Deal;

    public PhotonView PV;
    private void Awake()
    {
        StartCoroutine(Vanish(duration));
        Deal = SkillManager.instance.CaculateCharacterSkillDamage(casterSkillLevel, caseterPower,
            flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower,
            casterCriticalPercent, casterCriticalDamage, true);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        if (collision.CompareTag("Monster") && PV.IsMine)
        {
            PhotonView PV = collision.transform.GetComponent<PhotonView>();
            PV.RPC("MonsterDamage", RpcTarget.All, 0, Deal);
            //CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            //state.ProcessSkill(0, Deal);
        }

    }
}
