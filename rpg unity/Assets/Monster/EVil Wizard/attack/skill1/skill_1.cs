using UnityEngine;

public class skill_1 : MonoBehaviour
{
    public GameObject firePrefab; // �� ������
    private GameObject fire;      // ������ �� ������Ʈ
    public GameObject monster;
    public GameObject character;
    public Animator animator;     // �ִϸ����� ������Ʈ

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ExecuteSkill()
    {
        // �� �������� �̿��Ͽ� �� ����
        fire = Instantiate(firePrefab, transform.position, Quaternion.identity);

        if (fire != null)
        {
            // �� ������Ʈ�� �̵� �۾��� ����
            StartCoroutine(MoveFireRoutine());
        }
    }

    private System.Collections.IEnumerator MoveFireRoutine()
    {
        Transform monsterPosition = monster.transform; // ������ ��ġ (���� ��ġ)
        Transform characterPosition = character.transform;/* ĳ������ ��ġ Transform */; // ĳ������ ��ġ

        Vector3 startPosition = monsterPosition.position;
        Vector3 targetPosition = characterPosition.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (fire != null)
        {
            float distanceCovered = (Time.time - startTime) * 5.0f; // �̵� �ӵ� ���� ����
            float fractionOfJourney = distanceCovered / journeyLength;
            fire.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            if (fire.transform.position == targetPosition)
            {
                // ���� ĳ���� ��ġ�� �����ϸ� bomb Ʈ���� Ȱ��ȭ
                animator.SetTrigger("bomb");
                Destroy(fire);
                yield break;
            }

            yield return null;
        }
    }
}
