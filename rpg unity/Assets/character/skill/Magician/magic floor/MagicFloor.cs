using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MagicFloor : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private float cycle = 0.3f;
    private float time = 0;
    private bool active = false;


    private float flatDeal = 1;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;

    private float flatHeal = 1;
    private float healIncreasePerSkillLevel = 1;
    private float healIncreasePerPower = 1;

    private float duration = 3f;

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

    // Update is called once per frame
    void Update()
    {
        if (time > cycle) {
            active = true;
            time = 0;
        }
        else
        {
            active = false;
            time += Time.deltaTime;
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (active)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, "magic_floor", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
            }
        }
        else if (collision.CompareTag("Player"))
        {
            if (active)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(1, "magic_floor", flatHeal, healIncreasePerSkillLevel, healIncreasePerPower);
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("heal");
    }
}
