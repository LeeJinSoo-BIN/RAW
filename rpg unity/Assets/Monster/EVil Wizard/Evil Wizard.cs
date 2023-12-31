using JetBrains.Annotations;
using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class EvilWizard : MonoBehaviourPunCallbacks
{    
    //private skill_3 skill3;
    public GameObject fireprefab1;
    public GameObject fireprefab2;
    public GameObject fireObject3;
    public GameObject characterGroup;
    public Animator animator;
    private float time = 0f;
    private bool skillInvoked = false;
    //private bool mobControlEnabled = true; // MobControl ?????? ?????? ???????? ????
    private GameObject target;
    private Vector3 targetPos;
    private float patternCycle = 6f;
    public Transform staff;

    private Transform topLeft;
    private Transform bottomRight;

    public PhotonView PV;
    public bool attackable = true;
    public bool isDeath = false;
    private void Awake()
    {
        topLeft = GameObject.Find("ground").transform.Find("top left");
        bottomRight = GameObject.Find("ground").transform.Find("bottom right");
        characterGroup = GameObject.Find("Player Group");
        transform.parent = GameObject.Find("Enemy Group").transform;
    }

    private void Start()
    {        
        //skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && attackable && !isDeath)
        {
            time += Time.deltaTime;

            if (time >= patternCycle && skillInvoked == false) // 30???? ???? ?? ???? ????
            {
                //skillInvoked = true;
                //DisableMobControl(); // MobControl?? ????????
                //StartCoroutine(InvokeRandomSkillsRoutine());
                randomPattern();
                time = 0;
            }
            if(skillInvoked)
                time = 0;
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
        for(int k = 0; k < 10; k++)
        {
            spawnItem();
        }
        Destroy(gameObject, 0.45f);
    }

    void spawnItem()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-0.3f, 0.3f);
        Vector3 spawn_pos = new Vector3(x + transform.position.x, y + transform.position.y, 0);
        int rnd = Random.Range(0, 4);
        GameObject new_item = null;
        if (rnd < 3)
            new_item = PhotonNetwork.Instantiate("items/item_prefab", spawn_pos, Quaternion.identity);        
        if (rnd == 0)
            new_item.GetComponent<PhotonView>().RPC("initItem", RpcTarget.All, "red potion small", 1);
        else if(rnd == 1)
            new_item.GetComponent<PhotonView>().RPC("initItem", RpcTarget.All, "blue potion small", 1);
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
    private void findRandomPosition()
    {
        float x = Random.Range(topLeft.position.x,bottomRight.position.x);
        float y = Random.Range(bottomRight.position.y, topLeft.position.y);
        targetPos = new Vector3(x, y, 0);
    }
    private void skill1()//fireball
    {
        skillInvoked = true;
        GameObject fire = PhotonNetwork.Instantiate("Monster/Evil Wizard Skill1", staff.position, Quaternion.identity);
        // ????1 ???????? ?????????? ?????? ????
        findTarget();        
        fire.transform.GetComponent<PhotonView>().RPC("InitSkill", RpcTarget.All, staff.position, target.name);
        skillInvoked = false;
    }
    private void skill2()
    {           
        StartCoroutine(excuteSkill2());
    }
    IEnumerator excuteSkill2()//firehand
    {
        skillInvoked = true;
        findTarget();
        Vector2 characterPosition = target.transform.position;
        Vector2 monsterPosition = gameObject.transform.position;
        yield return StartCoroutine(MoveMonsterToCharacter(monsterPosition, characterPosition, 5f, true));
        GameObject fire = PhotonNetwork.Instantiate("Monster/Evil Wizard Skill2", target.transform.position, Quaternion.identity);
        EvilWizardSkill2 skill2 = fire.GetComponentInChildren<EvilWizardSkill2>();
        //skill2.character = target;
        skill2.ExecuteSkill();
        yield return StartCoroutine(MoveMonsterToCharacter(characterPosition, monsterPosition, 2f));
        skillInvoked = false;
    }
    
    void skill3()
    {        
        StartCoroutine(excuteSkill3());
    }
    IEnumerator excuteSkill3 ()
    {
        skillInvoked = true;
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
        StartCoroutine(excuteSkill4());
    }

    IEnumerator excuteSkill4()
    {
        skillInvoked = true;
        findRandomPosition();
        yield return MoveMonsterToCharacter(transform.position, targetPos, 3f);
        skillInvoked = false;
    }
    

    void skill5()
    {
        StartCoroutine(excuteSkill5());
    }
    IEnumerator excuteSkill5()
    {
        skillInvoked = true;
        animator.SetTrigger("spawn creature");
        float _time = 0f;
        while (_time < 0.3f)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        PhotonNetwork.Instantiate("Monster/Flying Eye", staff.position, Quaternion.identity);
        skillInvoked = false;
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
        int randomSkill = Random.Range(1, 6);
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
            case 4:
                skill4();
                break;
            case 5:
                skill5();
                break;
        }
    }

}
