using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MagicHeal : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update

    private float flatHeal = 3.5f;
    private float healIncreasePerSkillLevel = 1;
    private float healIncreasePerPower = 1;

    private float duration = 3f;
    public float caseterPower = 1f;
    public int casterSkillLevel = 1;
    float Heal;

    private GameObject target;

    public void excuteSKill()
    {
        CharacterState state = transform.parent.GetComponentInChildren<CharacterState>();
        Heal = SkillManager.instance.CaculateCharacterSkillDamage(casterSkillLevel, caseterPower,
            flatHeal, healIncreasePerSkillLevel, healIncreasePerPower);
        state.ProcessSkill(1, Heal);
    }

    private void Awake()
    {
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

    [PunRPC]
    void SetTarget(string targetName)
    {
        target = GameObject.Find(targetName);
        transform.parent = target.transform;
        //transform.localPosition = Vector3.zero;
        excuteSKill();
    }

    [PunRPC]
    void initSkill(float power, int skillLevel, float criticalPercent, float criticalDamage)
    {
        caseterPower = power;
        casterSkillLevel = skillLevel;
    }
}
