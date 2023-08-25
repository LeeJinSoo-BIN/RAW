using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowdrop : MonoBehaviour
{
    // Start is called before the first frame update
    private float flatDeal = 1;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;

    private float duration = 1.5f;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(0, "arrow_rain", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
        }

    }
}
