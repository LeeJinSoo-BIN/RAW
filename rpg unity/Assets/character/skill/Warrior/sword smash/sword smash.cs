using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSmash : MonoBehaviour
{
    private int flatDeal = 1;
    private int dealIncreasePerSkillLevel = 1;
    private int dealIncreasePerPower = 1;
    public GameObject target;

    private float duration = 0.7f;

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

    public void Deal()
    {
        CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(0, "sword_smash", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
    }
}
