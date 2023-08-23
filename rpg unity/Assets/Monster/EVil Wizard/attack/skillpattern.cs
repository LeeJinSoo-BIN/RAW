using JetBrains.Annotations;
using UnityEngine;

public class skillIpattern : MonoBehaviour
{
    private MobControl mobControl;
    private skill_2 skill2;
    private skill_3 skill3;
    public GameObject fireprefab;
    private float elapsedTime = 0.0f;
    private bool skillInvoked = false;
    private bool mobControlEnabled = true; // MobControl 활성화 여부를 저장하는 변수

    private void Start()
    {
        mobControl = GetComponent<MobControl>();
        skill2 = GetComponent<skill_2>();
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

    private void MakeFire()
    {
        GameObject fire = Instantiate(fireprefab, transform.position, Quaternion.identity);

        // 스킬1 클래스의 인스턴스를 만들고 실행
        skill_1 skill1 = fire.AddComponent<skill_1>();
        skill1.monster = gameObject;
        skill1.character = mobControl.character;
        skill1.ExecuteSkill();
    }

    private System.Collections.IEnumerator InvokeRandomSkillsRoutine()
    {
        while (true)
        {
            int randomSkill = Random.Range(1, 4);
            switch (randomSkill)
            {
                case 1:
                    MakeFire();
                    break;
                case 2:
                    skill2.ExecuteSkill();
                    break;
                case 3:
                    skill3.ExecuteSkill();
                    break;
            }

            yield return new WaitForSeconds(3.0f); // 스킬 호출 간격 (초)
        }
    }
}
