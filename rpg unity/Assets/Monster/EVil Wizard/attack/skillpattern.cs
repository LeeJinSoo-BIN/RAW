using JetBrains.Annotations;
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class skillIpattern : MonoBehaviourPunCallbacks
{    
    //private skill_3 skill3;
    public GameObject fireprefab1;
    public GameObject fireprefab2;
    public GameObject fireObject3;
    public GameObject characterGroup;
    public Animator animator;
    private float _time = 0.0f;
    private bool skillInvoked = false;
    //private bool mobControlEnabled = true; // MobControl ?????? ?????? ???????? ????
    private GameObject target;
    private float patternCycle = 6f;
    public Transform staff;

    private Transform topLeft;
    private Transform bottomRight;

    public PhotonView PV;
    public bool attackable = true;
    public bool isDeath = false;
    private void Awake()
    {
        transform.parent = GameObject.Find("Enemy Group").transform;
        characterGroup = GameObject.Find("Player Group");
    }

    private void Start()
    {        
        //skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && attackable && !isDeath)
        {
            _time += Time.deltaTime;

            if (_time >= patternCycle && skillInvoked == false) // 30???? ???? ?? ???? ????
            {
                //skillInvoked = true;
                //DisableMobControl(); // MobControl?? ????????
                //StartCoroutine(InvokeRandomSkillsRoutine());
                randomPattern();
                _time = 0;
            }
            //StartCoroutine(InvokeRandomSkillsRoutine());
        }
    }
        

    /*private void DisableMobControl()
    {
        if (mobControlEnabled)
        {
            mobControl.enabled = false; // MobControl ????????
            mobControlEnabled = false;
        }
    }*/
    public void Death()
    {
        attackable = false;
        isDeath = true;
        animator.SetTrigger("Death");
    }

    public void Bind(float bindTime = 5f)
    {
        if(!isDeath)
            StartCoroutine(ExcuteBind(bindTime));
    }
    public void Hit()
    {
        if(!isDeath)
            animator.SetTrigger("hit");
    }

    IEnumerator ExcuteBind(float bindTime)
    {
        attackable = false;
        animator.SetBool("isBound", true);
        animator.SetTrigger("hit");
        float _time = 0f;
        while(_time < bindTime)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        animator.SetBool("isBound", false);
        attackable = true;
    }

    private void findTarget()
    {
        int numCharacter = characterGroup.transform.childCount;
        int who = Random.Range(0, numCharacter);
        target = characterGroup.transform.GetChild(who).gameObject;

    }
    private void skill1()
    {
        skillInvoked = true;
        GameObject fire = PhotonNetwork.Instantiate("skill1", staff.position, Quaternion.identity);
        // ????1 ???????? ?????????? ?????? ????
        findTarget();
        skill_1 skill1 = fire.GetComponent<skill_1>();
        fire.transform.GetComponent<PhotonView>().RPC("InitSkill", RpcTarget.All, staff.position, target.name);
        //skill1.startPosition = staff.position;
        //skill1.character = target;
        //skill1.ExecuteSkill();
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
        GameObject fire = PhotonNetwork.Instantiate("skill2", target.transform.position, Quaternion.identity);
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
        yield return StartCoroutine(MoveMonsterToCharacter(monsterPosition, targetPosition, 3.5f, distance:0.001f, run: true));
        if (characterPosition.x < transform.position.x && transform.localScale.x > 0)
            transform.localScale = new Vector3(-3, 3);
        else if (characterPosition.x > transform.position.x && transform.localScale.x < 0)
            transform.localScale = new Vector3(3, 3);
        animator.SetTrigger("attack");
        float _time = 0f;
        while(_time < 0.05f)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        _time = 0f;
        fireObject3.SetActive(true);
        while (_time < 0.5f)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        fireObject3.SetActive(false);
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
            if (targetPosition.x < transform.position.x && transform.localScale.x > 0)
                transform.localScale = new Vector3(-3, 3);
            else if (targetPosition.x > transform.position.x && transform.localScale.x < 0)
                transform.localScale = new Vector3(3, 3);                
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }
        if (run)
            animator.SetBool("run", false);
        // ???? ???? ?? ???? ???? (?????? ???? ????)
        //onComplete?.Invoke();
    }

    void randomPattern()
    {
        int randomSkill = Random.Range(1, 4);
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
