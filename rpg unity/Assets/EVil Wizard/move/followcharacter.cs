using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobControl : MonoBehaviour
{
    public GameObject character; // 캐릭터 GameObject에 대한 참조
    public float moveSpeed = 3f; // 몹의 이동 속도
    public float followDistance = 5f; // 캐릭터를 따라가기 시작할 거리
    public float stoppingDistance = 2f; // 멈추기 위한 거리

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 몹의 Rigidbody2D 컴포넌트 가져오기
        character = GameObject.Find("Character"); // 씬에서 "Character"라는 이름을 가진 캐릭터 GameObject 찾기
    }

    // Update is called once per frame
    void Update()
    {
        if (character != null) // 캐릭터 GameObject 참조가 null이 아닌지 확인 (캐릭터가 씬에 존재하는지)
        {
            float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position); // 몹과 캐릭터 사이의 거리 계산

            if (distanceToCharacter <= followDistance && distanceToCharacter > stoppingDistance) // 캐릭터와의 거리가 followDistance 이내이면서 stoppingDistance보다 크다면
            {
                Vector3 targetPosition = new Vector3(character.transform.position.x, character.transform.position.y, transform.position.z); // 몹과 동일한 z축을 가지면서 캐릭터의 x와 y 위치를 가진 targetPosition 생성
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime); // Time.deltaTime을 사용하여 moveSpeed의 일정 속도로 targetPosition으로 이동
            }
        }
    }
}
