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

    private void Start()
    {
        mobControl = GetComponent<MobControl>();
        
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

    private void makefire()
    {
        GameObject fire = Instantiate(fireprefab, transform.position, Quaternion.identity); ;
       
        fire.GetComponent<skill_1>().monster = gameObject;
        fire.GetComponent<skill_1>().character = mobControl.character;
        fire.GetComponent<skill_1>().ExecuteSkill();
    }

    private System.Collections.IEnumerator InvokeRandomSkillsRoutine()
    {
        while (true)
        {
            int randomSkill = Random.Range(1, 4);
            switch (randomSkill)
            {
                case 1:
                    makefire();
                    
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