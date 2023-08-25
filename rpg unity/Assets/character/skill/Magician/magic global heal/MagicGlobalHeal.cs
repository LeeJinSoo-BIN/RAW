using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicGlobalHeal : MonoBehaviour
{
    // Start is called before the first frame update
    private float flatHeal = 5;
    private float healIncreasePerSkillLevel = 1;
    private float healIncreasePerPower = 1;

    private float duration = 3f;
    public float caseterPower = 1f;
    public float casterSkillLevel = 1f;

    private float Heal;
    void Start()
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

}