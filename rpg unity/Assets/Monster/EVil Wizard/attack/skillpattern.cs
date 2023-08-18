using UnityEngine;

public class skillIpattern : MonoBehaviour
{
    private MobControl mobControl;
    private skill_1 skill1;
    private skill_2 skill2;
    private skill_3 skill3;

    private float elapsedTime = 0.0f;
    private bool skillInvoked = false;

    private void Start()
    {
        mobControl = GetComponent<MobControl>();
        skill1 = GetComponent<skill_1>();
        skill2 = GetComponent<skill_2>();
        skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (!skillInvoked && elapsedTime >= 10.0f)
        {
            InvokeRandomSkillsContinuously();
            skillInvoked = true;
        }
    }

    private void InvokeRandomSkillsContinuously()
    {
        StartCoroutine(InvokeRandomSkillsRoutine());
    }

    private System.Collections.IEnumerator InvokeRandomSkillsRoutine()
    {
        while (true)
        {
            int randomSkill = Random.Range(1, 4);
            switch (randomSkill)
            {
                case 1:
                    skill1.ExecuteSkill();
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