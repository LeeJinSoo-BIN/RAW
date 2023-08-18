using UnityEngine;

public class skill_1 : MonoBehaviour
{
    public GameObject firePrefab; // 불 프리팹
    private GameObject fire;      // 생성된 불 오브젝트
    public GameObject monster;
    public GameObject character;
    public Animator animator;     // 애니메이터 컴포넌트

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ExecuteSkill()
    {
        // 불 프리팹을 이용하여 불 생성
        fire = Instantiate(firePrefab, transform.position, Quaternion.identity);

        if (fire != null)
        {
            // 불 오브젝트의 이동 작업을 수행
            StartCoroutine(MoveFireRoutine());
        }
    }

    private System.Collections.IEnumerator MoveFireRoutine()
    {
        Transform monsterPosition = monster.transform; // 몬스터의 위치 (현재 위치)
        Transform characterPosition = character.transform;/* 캐릭터의 위치 Transform */; // 캐릭터의 위치

        Vector3 startPosition = monsterPosition.position;
        Vector3 targetPosition = characterPosition.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (fire != null)
        {
            float distanceCovered = (Time.time - startTime) * 5.0f; // 이동 속도 조정 가능
            float fractionOfJourney = distanceCovered / journeyLength;
            fire.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            if (fire.transform.position == targetPosition)
            {
                // 불이 캐릭터 위치에 도달하면 bomb 트리거 활성화
                animator.SetTrigger("bomb");
                Destroy(fire);
                yield break;
            }

            yield return null;
        }
    }
}
