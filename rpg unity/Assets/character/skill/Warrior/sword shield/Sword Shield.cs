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

    private void Awake()
    {
        target = transform.parent.gameObject;
        Shield();
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

    public void Shield()
    {
        CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(2, "sword_shield", flatShield, shieldIncreasePerSkillLevel, shieldIncreasePerPower);
    }
}
