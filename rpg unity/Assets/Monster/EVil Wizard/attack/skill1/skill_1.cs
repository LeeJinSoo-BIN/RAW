using UnityEditor;
using UnityEngine;

public class skill_1 : MonoBehaviour
{
    
    public GameObject monster;
    public GameObject character;
    private Animator animator;     // �ִϸ����� ������Ʈ
    private float speed = 10f;
    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

   
    public void ExecuteSkill()
    {
        // �� ������Ʈ�� �̵� �۾��� ����
        StartCoroutine(MoveFireRoutine());        
    }

    private System.Collections.IEnumerator MoveFireRoutine()
    {        
        Vector3 startPosition = monster.transform.position;// ������ ��ġ (���� ��ġ)
        Vector3 targetPosition = character.transform.position;/* ĳ������ ��ġ Transform */
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;


        while (true)
        {
            float distanceCovered = (Time.time - startTime) * speed; // �̵� �ӵ� ���� ����
            float fractionOfJourney = distanceCovered / journeyLength;
            gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            if ((gameObject.transform.position - targetPosition).magnitude <= 0.1f)
            {
                // ���� ĳ���� ��ġ�� �����ϸ� bomb Ʈ���� Ȱ��ȭ
                animator.SetTrigger("bomb");
                break;
            }
            yield return null;
        }
        while (true)
        {
            if (Time.time - startTime >= 3.0f)
            {
                Destroy(gameObject);
                break;
            }
            yield return null;
        }
    }
}
