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
    //private bool mobControlEnabled = true; // MobControl 활성화 여부를 저장하는 변수
    private GameObject target;
    public float patternCycle = 5f;
    public Transform staff;
    private void Start()
    {        
        //skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        _time += Time.deltaTime;

        if ( _time >= patternCycle && skillInvoked == false) // 30초가 지난 후 스킬 사용
        {
            //skillInvoked = true;
            //DisableMobControl(); // MobControl을 비활성화
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
            mobControl.enabled = false; // MobControl 비활성화
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
        skillInvoked = true;
        GameObject fire = Instantiate(fireprefab1, staff.position, Quaternion.identity);
        // 스킬1 클래스의 인스턴스를 만들고 실행
        findTarget();
        skill_1 skill1 = fire.GetComponent<skill_1>();
        skill1.startPosition = staff.position;
        skill1.character = target;
        skill1.ExecuteSkill();
        skillInvoked = false;
    }
    private void skill2()
    {   
        skillInvoked=true;
        StartCoroutine(excuteSkill2());
    }
    IEnumerator excuteSkill2()
    {
        
        findTarget();
        Vector2 characterPosition = target.transform.position;
        Vector2 monsterPosition = gameObject.transform.position;
        yield return StartCoroutine(MoveMonsterToCharacter(monsterPosition, characterPosition, 5f, true));
        GameObject fire = Instantiate(fireprefab2, target.transform.position, Quaternion.identity);
        skill_2 skill2 = fire.GetComponentInChildren<skill_2>();
        //skill2.character = target;
        skill2.ExecuteSkill();
        yield return StartCoroutine(MoveMonsterToCharacter(characterPosition, monsterPosition, 2f));
        skillInvoked = false;
    }
    
    void skill3()
    {
        skillInvoked = true;
        StartCoroutine(excuteSkill3());
    }
    IEnumerator excuteSkill3 ()
    {
        findTarget();
        Vector2 characterPosition = target.transform.position;
        float where = 1f;
        if(Random.Range(0,2) == 1)
            where = -1f;
        Vector2 targetPosition = new Vector2(characterPosition.x + where, characterPosition.y);
        Vector2 monsterPosition = gameObject.transform.position;
        yield return StartCoroutine(MoveMonsterToCharacter(monsterPosition, targetPosition, 5f, distance:0.001f, run: true));
        if(characterPosition.x < transform.position.x && transform.position.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if(characterPosition.x > transform.position.x && transform.position.x < 0)
            transform.localScale = new Vector3(1, 1, 1);
        animator.SetTrigger("attack");
        skillInvoked = false;
    }

    void skill4()
    {
        skillInvoked = true;

    }
    

    private IEnumerator MoveMonsterToCharacter(Vector3 startPosition, Vector3 targetPosition, float speed, bool run = false, float distance = 2.0f)
    {
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        if (run)
            animator.SetBool("run", true);
        while ((transform.position - targetPosition).magnitude > distance)
        {
            if (targetPosition.x < transform.position.x && transform.position.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (targetPosition.x > transform.position.x && transform.position.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }
        if (run)
            animator.SetBool("run", false);
        // 이동 완료 후 콜백 실행 (콜백이 있을 경우)
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
                    skill3();
                    break;
            }

            yield return new WaitForSeconds(3.0f); // 스킬 호출 간격 (초)
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
                Debug.Log("skill 3");
                skill3();
                break;
        }
    }
}
