using UnityEngine;

public class skill_1 : MonoBehaviour
{
    
    public GameObject monster;
    public GameObject character;
    private Animator animator;     // �ִϸ����� ������Ʈ

    private void Start()
    {
        
    }

   
    public void ExecuteSkill()
    {
        
        animator = gameObject.GetComponent<Animator>();
       
            // �� ������Ʈ�� �̵� �۾��� ����
        StartCoroutine(MoveFireRoutine());
        
    }

    private System.Collections.IEnumerator MoveFireRoutine()
    {
        Transform monsterPosition = monster.transform; // ������ ��ġ (���� ��ġ)
        Transform characterPosition = character.transform;/* ĳ������ ��ġ Transform */
        Vector3 startPosition = monsterPosition.position;
        Vector3 targetPosition = characterPosition.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;


        while (gameObject != null)
        {

            float distanceCovered = (Time.time - startTime) * 10.0f; // �̵� �ӵ� ���� ����
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
            if (Time.time - startTime >= 10.0f)
            {
                Destroy(gameObject);
                break;
            }
            yield return null;

        }
    }
}
