using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicHeal : MonoBehaviour
{
    // Start is called before the first frame update

    private int flatHeal = 1;
    private int healIncreasePerSkillLevel = 1;
    private int healIncreasePerPower = 1;

    private float duration = 3f;

    void Start()
    {
        CharacterState state = transform.parent.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(1, "magic_heal", flatHeal, healIncreasePerSkillLevel, healIncreasePerPower);
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
