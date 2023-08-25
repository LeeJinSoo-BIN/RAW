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
        // ĳ������ ��ġ�� �� ����

        if (gameObject != null)
        {
            // �� �ִϸ����͸� Ȱ��ȭ
            animator.SetBool("Explode", true);

            // �� ������Ʈ �ı� (�ִϸ��̼� �̺�Ʈ�� �ı��� ���� ����)
            Destroy(gameObject, 2.0f); // ��: 2�� �ڿ� �ı�
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

