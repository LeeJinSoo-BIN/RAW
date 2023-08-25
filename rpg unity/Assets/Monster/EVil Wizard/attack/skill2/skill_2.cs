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
    private bool hand = false;
    private bool bomb = false;
    private void Start()
    {
        //animator = GetComponent<Animator>();
    }

    public void ExecuteSkill()
    {
        //StartCoroutine(FireHand());
    }

    /*private IEnumerator FireHand()
    {
        // 캐릭터의 위치로 불 생성

        if (gameObject != null)
        {
            // 불 애니메이터를 활성화
            animator.SetBool("Explode", true);

            // 불 오브젝트 파괴 (애니메이션 이벤트로 파괴할 수도 있음)
            Destroy(gameObject, 2.0f); // 예: 2초 뒤에 파괴
        }

        yield return null;
    }*/

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

