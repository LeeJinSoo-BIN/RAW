using JetBrains.Annotations;
using UnityEngine;
using System.Collections;

public class skillIpattern : MonoBehaviour
{    
    //private skill_3 skill3;
    public GameObject fireprefab1;
    public GameObject fireprefab2;
    public GameObject characterGroup;
    public Animator animator;
    private float _time = 0.0f;
    private bool skillInvoked = false;
    //private bool mobControlEnabled = true; // MobControl Ȱ��ȭ ���θ� �����ϴ� ����
    private GameObject target;
    public float patternCycle = 5f;
    private void Start()
    {        
        //skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        _time += Time.deltaTime;

        if ( _time >= patternCycle) // 30�ʰ� ���� �� ��ų ���
        {
            //skillInvoked = true;
            //DisableMobControl(); // MobControl�� ��Ȱ��ȭ
            //StartCoroutine(InvokeRandomSkillsRoutine());
            randomPattern();
            _time = 0;
        }
        //StartCoroutine(InvokeRandomSkillsRoutine());
    }

    /*private void DisableMobControl()
    {
        if (mobControlEnabled)
        {
            mobControl.enabled = false; // MobControl ��Ȱ��ȭ
            mobControlEnabled = false;
        }
    }*/

    private void findTarget()
    {
        int numCharacter = characterGroup.transform.childCount;
        int who = Random.Range(0, numCharacter);
        target = characterGroup.transform.GetChild(who).gameObject;

    }
    private void skill1()
    {
        GameObject fire = Instantiate(fireprefab1, transform.position, Quaternion.identity);
        // ��ų1 Ŭ������ �ν��Ͻ��� ����� ����
        findTarget();
        skill_1 skill1 = fire.GetComponent<skill_1>();
        skill1.monster = gameObject;
        skill1.character = target;
        skill1.ExecuteSkill();
    }
    private void skill2()
    {        
        StartCoroutine(excuteSKill2());
    }
    IEnumerator excuteSKill2()
    {
        findTarget();
        Vector2 characterPosition = target.transform.position;
        Vector2 monsterPosition = gameObject.transform.position;
        yield return StartCoroutine(MoveMonsterToCharacter(monsterPosition, characterPosition, 5f, true));
        GameObject fire = Instantiate(fireprefab2, target.transform.position, Quaternion.identity);
        skill_2 skill2 = fire.GetComponent<skill_2>();
        //skill2.character = target;
        skill2.ExecuteSkill();
        StartCoroutine(MoveMonsterToCharacter(characterPosition, monsterPosition, 2f));
    }
    
    private IEnumerator MoveMonsterToCharacter(Vector3 startPosition, Vector3 targetPosition, float speed, bool run = false, float distance = 2.0f)
    {
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        if (run)
            animator.SetBool("run", true);
        while ((transform.position - targetPosition).magnitude > distance)
        {
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }
        if (run)
            animator.SetBool("run", false);
        // �̵� �Ϸ� �� �ݹ� ���� (�ݹ��� ���� ���)
        //onComplete?.Invoke();
    }


    IEnumerator InvokeRandomSkillsRoutine()
    {
        while (true)
        {
            int randomSkill = Random.Range(1, 2);
            switch (randomSkill)
            {
                case 1:
                    skill1();
                    break;
                case 2:
                    skill2();
                    break;
                case 3:
                    //skill3.ExecuteSkill();
                    break;
            }

            yield return new WaitForSeconds(3.0f); // ��ų ȣ�� ���� (��)
        }
    }

    void randomPattern()
    {
        int randomSkill = Random.Range(1, 2);
        switch (randomSkill)
        {
            case 1:
                Debug.Log("skill 1");
                skill1();
                break;
            case 2:
                Debug.Log("skill 2");
                skill2();
                break;
            case 3:                
                //skill3.ExecuteSkill();
                break;
        }
    }
}
