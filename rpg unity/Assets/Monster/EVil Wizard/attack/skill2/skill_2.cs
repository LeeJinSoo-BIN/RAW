using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class skill_2 : MonoBehaviour
{
    //public GameObject character; // ĳ������ ��ġ
    //private Animator animator; // �ִϸ����� ������Ʈ

    private float flatHandDeal = 50f;
    private float flatBombDeal = 50f;
    private float level = 1;
    public bool hand = false;
    public bool bomb = false;

    public void ExecuteSkill()
    {
        StartCoroutine(Vanish());
    }

    private IEnumerator Vanish()
    {
        // ĳ������ ��ġ�� �� ����

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
        Debug.Log(collision.name);
        if (collision.CompareTag("Player"))
        {
            if (hand)
            {
                Debug.Log("hand");
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, flatHandDeal * level);
            }
            if (bomb)
            {
                Debug.Log("bomb");
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, flatBombDeal * level);
            }
        }
    }

}

