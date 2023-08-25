using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordShield : MonoBehaviour
{
    private float flatShield = 1;
    private float shieldIncreasePerSkillLevel = 1;
    private float shieldIncreasePerPower = 1;
    public GameObject target;

    private float duration = 0.7f;

    public float caseterPower = 1f;
    public float casterSkillLevel = 1f;
    public float casterCriticalPercent = 1f;
    public float casterCriticalDamage = 1f;
    public float Shield;
    private void Awake()
    {
        target = transform.parent.gameObject;
        Shield = SkillManager.instance.CaculateCharacterSkillDamage(casterSkillLevel, caseterPower,
            flatShield, shieldIncreasePerSkillLevel, shieldIncreasePerPower,
            casterCriticalPercent, casterCriticalDamage, true);
        gainShield();
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

    public void gainShield()
    {
        CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(2, Shield);
    }
}
