using UnityEngine;
using System.Collections;

public class skill_2 : MonoBehaviour
{
    //public GameObject character; // ĳ������ ��ġ
    //private Animator animator; // �ִϸ����� ������Ʈ
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
}
