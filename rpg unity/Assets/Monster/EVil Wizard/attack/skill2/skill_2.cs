using UnityEngine;

public class skill_2 : MonoBehaviour
{
    public GameObject monster;       // 몬스터 GameObject
    public GameObject character;     // 캐릭터 GameObject
    public float moveSpeed = 5.0f;   // 이동 속도

    private Vector3 targetPosition;  // 이동할 목표 위치
    private bool isMoving = false;   // 이동 중 여부

    public void ExecuteSkill()
    {
        // 몬스터의 현재 위치를 캐릭터 근처로 설정
        targetPosition = character.transform.position;
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            // 현재 위치에서 목표 위치로 이동
            float step = moveSpeed * Time.deltaTime;
            monster.transform.position = Vector3.MoveTowards(monster.transform.position, targetPosition, step);

            // 목표 위치에 도달하면 이동 중지
            if (monster.transform.position == targetPosition)
            {
                isMoving = false;
            }
        }
    }
}
