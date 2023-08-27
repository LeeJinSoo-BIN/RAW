using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class skill_2 : MonoBehaviour
{
    //public GameObject character; // 캐릭터의 위치
    //private Animator animator; // 애니메이터 컴포넌트

    private float flatHandDeal = 50f;
    private float flatBombDeal = 50f;
    private float level = 1;
    public bool hand = false;
    public bool bomb = false;

    public void Start()
    {
        StartCoroutine(Vanish(10f));

    }
    public void ExecuteSkill()
    {
        StartCoroutine(Fire());
    }

    IEnumerator Vanish(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        //GetComponent<Animator>().SetTrigger("vanish");
        Destroy(gameObject, 0.45f);
    }

    private IEnumerator Fire()
    {
        // 캐릭터의 위치로 불 생성

        while (true)
        {
            if(bomb)
            {
                float time = 0f;
                while(true)
                {
                    time += Time.deltaTime;
                    if(time > 0.5f)
                    {
                        Destroy(gameObject);
                        break;
                    }
                    yield return null;
                }
            }
            yield return null;
        }

        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {        
        if (collision.CompareTag("Player"))
        {
            if (hand)
            {                
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, flatHandDeal * level);
            }
            if (bomb)
            {                
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, flatBombDeal * level);
            }
        }
    }

}

