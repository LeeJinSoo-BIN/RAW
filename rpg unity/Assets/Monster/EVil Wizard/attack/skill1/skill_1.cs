using UnityEditor;
using UnityEngine;

public class skill_1 : MonoBehaviour
{
    
    public GameObject monster;
    public GameObject character;
    private Animator animator;     // 애니메이터 컴포넌트
    private float speed = 10f;
    private void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

   
    public void ExecuteSkill()
    {
        // 불 오브젝트의 이동 작업을 수행
        StartCoroutine(MoveFireRoutine());        
    }

    private System.Collections.IEnumerator MoveFireRoutine()
    {        
        Vector3 startPosition = monster.transform.position;// 몬스터의 위치 (현재 위치)
        Vector3 targetPosition = character.transform.position;/* 캐릭터의 위치 Transform */
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;


        while (true)
        {
            float distanceCovered = (Time.time - startTime) * speed; // 이동 속도 조정 가능
            float fractionOfJourney = distanceCovered / journeyLength;
            gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            if ((gameObject.transform.position - targetPosition).magnitude <= 0.1f)
            {
                // 불이 캐릭터 위치에 도달하면 bomb 트리거 활성화
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
