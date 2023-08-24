using JetBrains.Annotations;
using UnityEngine;
using System.Collections;

public class skillIpattern : MonoBehaviour
{
    private MobControl mobControl;
    private skill_2 skill2;
    private skill_3 skill3;
    public GameObject fireprefab1;
    public GameObject fireprefab2;
    public GameObject character;
    private float elapsedTime = 0.0f;
    private bool skillInvoked = false;
    private bool mobControlEnabled = true; // MobControl 활성화 여부를 저장하는 변수

    private void Start()
    {
        mobControl = GetComponent<MobControl>();
        skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (!skillInvoked && elapsedTime >= 30.0f) // 30초가 지난 후 스킬 사용
        {
            skillInvoked = true;
            DisableMobControl(); // MobControl을 비활성화
            InvokeRandomSkillsContinuously();
        }
    }

    private void DisableMobControl()
    {
        if (mobControlEnabled)
        {
            mobControl.enabled = false; // MobControl 비활성화
            mobControlEnabled = false;
        }
    }

    private void InvokeRandomSkillsContinuously()
    {
        StartCoroutine(InvokeRandomSkillsRoutine());
    }

    private void skill1()
    {
        GameObject fire = Instantiate(fireprefab1, transform.position, Quaternion.identity);

        // 스킬1 클래스의 인스턴스를 만들고 실행
        skill_1 skill1 = fire.AddComponent<skill_1>();
        skill1.monster = gameObject;
        skill1.character = mobControl.character;
        skill1.ExecuteSkill();
    }
    private void skill2()
    {
        GameObject fire = Instantiate(fireprefab2, transform.position, Quaternion.identity);

        skill_2 skill2 = fire.AddComponent<skill_2>();
        skill2.character = mobControl.character;
        private Transform characterPosition = character.transform.position;
    private Transform monsterPosition = gameObject.transform.position;

    StartCoroutine(MoveMonsterToCharacter(monsterPosition, characterPosition, () =>
    {
        // 이동이 완료되면 스킬 2 실행
        skill2.ExecuteSkill();

        // 스킬 실행 후 멀리 떨어지기 위해 다시 함수 호출
        StartCoroutine(MoveMonsterToCharacter(characterPosition, monsterPosition, null));
    }));
    
    private IEnumerator MoveMonsterToCharacter(Vector3 startPosition, Vector3 targetPosition, System.Action onComplete)
    {
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (transform.position != targetPosition)
        {
            float distanceCovered = (Time.time - startTime) * 5.0f;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }

        // 이동 완료 후 콜백 실행 (콜백이 있을 경우)
        onComplete?.Invoke();
    }


    private System.Collections.IEnumerator InvokeRandomSkillsRoutine()
    {
        while (true)
        {
            int randomSkill = Random.Range(1, 4);
            switch (randomSkill)
            {
                case 1:
                    skill1();
                    break;
                case 2:
                    skill2();
                    break;
                case 3:
                    skill3.ExecuteSkill();
                    break;
            }

            yield return new WaitForSeconds(3.0f); // 스킬 호출 간격 (초)
        }
    }
}
