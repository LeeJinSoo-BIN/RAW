using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTotem : MonoBehaviour
{
    float dropTime = 0.3f;
    float _time = 0f;

    private float flatDeal = 1;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;

    private float flatPower = 1;
    private float powerIncreasePerSkillLevel = 1;

    public GameObject aura;

    private float duration = 10f;

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

    void Start()
    {
        StartCoroutine(clock());
    }

    // Update is called once per frame
    
    IEnumerator clock()
    {
        while(_time <= dropTime)
        {
            _time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (_time <= dropTime)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, "magic_totem", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
            }
        }
        else if (collision.CompareTag("Player"))
        {
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(3, "magic_totem", flatPower, powerIncreasePerSkillLevel);
        }
        Debug.Log(collision.name);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(3, "magic_totem", flatPower, powerIncreasePerSkillLevel, positive: false);
        }
    }
    
}
