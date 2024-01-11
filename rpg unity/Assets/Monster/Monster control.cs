using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterControl : MonoBehaviour
{
    public MonsterSpec monsterSpec;
    public GameObject canvas;
    private GameObject characterGroup;
    private Transform topLeft;
    private Transform bottomRight;

    public int level = 1;
    public bool attackable = true;
    public bool isDeath = false;
    private bool isMovable = true;
    private float time = 0f;
    private bool isMoving = false;
    private bool isCastingSkill = false;
    private float patternCycle;
    private bool isAggro = false;
    private IEnumerator currentCastingSkillCoroutine;
    private IEnumerator runWhileCastingSkillCoroutine;
    private MonsterSkillSpec currentCastingSkill;

    private GameObject target;
    private Vector3 targetPos;

    public PhotonView PV;
    public Animator animator;

    private GameManager gameManager;
    
    private void Awake()
    {
        topLeft = GameObject.Find("map").transform.Find("left top");
        bottomRight = GameObject.Find("map").transform.Find("right bottom");
        characterGroup = GameObject.Find("Player Group");
        transform.parent = GameObject.Find("Enemy Group").transform;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        name += transform.parent.childCount;
        patternCycle = monsterSpec.patternCycle[DataBase.Instance.currentDungeonLevel];
        currentCastingSkill = new MonsterSkillSpec();
    }

    private void Start()
    {

    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && attackable && !isDeath)
        {
            if (isAggro)
            {                
                MoveToTarget(0.01f, 0.1f);
                return;
            }
            time += Time.deltaTime;
            if (time >= patternCycle && !isCastingSkill) // 30???? ???? ?? ???? ????
            {
                isMoving = false;
                if (monsterSpec.haveWalkMotion)
                    animator.SetBool("walk", false);
                randomPattern();
                time = 0;
            }
            else if (!isCastingSkill)
            {
                if (target != null && monsterSpec.defaultMovement == "following")
                    MoveToTarget(2, 5);
            }
            if (isCastingSkill)
                time = 0;
        }
    }

    void randomPattern()
    {
        int rnd = Random.Range(0, monsterSpec.skillList.Count);
        currentCastingSkill = monsterSpec.skillList[rnd];
        if (currentCastingSkill.type == "MoveRandom")
            currentCastingSkillCoroutine = moveRandom();
        else if (currentCastingSkill.type == "Follow")
            currentCastingSkillCoroutine = follow();
        else if (currentCastingSkill.type == "FindTarget")
            findTarget();
        else
            currentCastingSkillCoroutine = castSKill();

        StartCoroutine(currentCastingSkillCoroutine);
    }

    IEnumerator moveRandom()
    {
        isCastingSkill = true;
        findRandomPosition();
        runWhileCastingSkillCoroutine = MoveToTargetCoroutine(transform.position, targetPos, currentCastingSkill.speed, currentCastingSkill.run);
        yield return StartCoroutine(runWhileCastingSkillCoroutine);
        isCastingSkill = false;
    }
    IEnumerator follow()
    {
        isCastingSkill = true;
        findTarget();
        float _time = 0f;
        while (_time < currentCastingSkill.skillDuration)
        {
            _time += Time.deltaTime;
            MoveToTarget(2, 5);
            yield return null;
        }
        isCastingSkill = false;

    }
    IEnumerator castSKill()
    {
        isCastingSkill = true;
        findTarget();
        Vector3 currentTargetPos = targetPos;
        float currentMoveSpeed = 1f;
        if (currentCastingSkill.castPos == "side")
        {
            float where = currentCastingSkill.targetSideDistance;
            if (Random.Range(0, 2) == 1)
                where *= -1;
            currentTargetPos = new Vector2(currentTargetPos.x + where, currentTargetPos.y);
        }
        else if (currentCastingSkill.castPos == "instance")
            currentTargetPos = transform.position;
        currentMoveSpeed = currentCastingSkill.speed;

        runWhileCastingSkillCoroutine = MoveToTargetCoroutine(transform.position, currentTargetPos, currentMoveSpeed, currentCastingSkill.run, currentCastingSkill.distance);
        yield return StartCoroutine(runWhileCastingSkillCoroutine);

        Vector3 _dirMVec = (target.transform.position - transform.position).normalized;
        PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);

        float _time = 0f;
        try
        {
            animator.SetTrigger(currentCastingSkill.skillName);
        }
        catch
        {
            Debug.Log("doesn't have parameter");
        }
        while (true)
        {
            if (_time > currentCastingSkill.firstDelay)
                break;
            _time += Time.deltaTime;
            yield return null;
        }        
        if (currentCastingSkill.type == "spawn" || currentCastingSkill.type == "spawn creature")
            yield return StartCoroutine(spawnAttack());
        else if (currentCastingSkill.type == "direct")
            yield return StartCoroutine(directAttack());

        isCastingSkill = false;
    }

    IEnumerator spawnAttack()
    {
        Vector3 spawnPos = Vector3.zero;
        if (currentCastingSkill.spawnPos == "target")
            spawnPos = target.transform.position;
        else
            spawnPos = transform.Find(currentCastingSkill.spawnPos).position;
        GameObject spawnObject = PhotonNetwork.InstantiateRoomObject(currentCastingSkill.skillDirectory, spawnPos, Quaternion.identity);        
        int numDeal = currentCastingSkill.flatDeal.Length;
        float[] deal = new float[numDeal];
        for(int k = 0; k < currentCastingSkill.flatDeal.Length; k++)
        {
            deal[k] = currentCastingSkill.flatDeal[k] + currentCastingSkill.increaseDealPerLevel[k] * level;
        }
        if(currentCastingSkill.type == "spawn")
            spawnObject.GetComponent<PhotonView>().RPC("InitSkill", RpcTarget.All, spawnPos, target.name, deal);
        float _time = 0f;
        while (_time < currentCastingSkill.skillDuration)
        {
            _time += Time.deltaTime;
            yield return null;
        }        
    }
    IEnumerator directAttack()
    {   
        int deal = (int)(currentCastingSkill.flatDeal[0] + currentCastingSkill.increaseDealPerLevel[0] * level);
        PV.RPC("setActiveCollider", RpcTarget.All, currentCastingSkill.areaChildNum, true, deal);
        float _time = 0f;
        while (true)
        {
            if (_time > currentCastingSkill.skillDuration)
                break;
            _time += Time.deltaTime;
            yield return null;
        }
        PV.RPC("setActiveCollider", RpcTarget.All, currentCastingSkill.areaChildNum, false, deal);
    }

    [PunRPC]
    void setActiveCollider(int childNum, bool onOff, int deal)
    {
        GameObject area = transform.GetChild(childNum).gameObject;
        area.GetComponent<MonsterDirectAttack>().dealOnce = onOff;
        area.GetComponent<MonsterDirectAttack>().Deal = deal;        
        area.SetActive(onOff);        
    }

    [PunRPC]
    void setCreatureName(string newName)
    {
        name = newName;
    }

    [PunRPC]
    public void aggro(string targetName, float aggroTime)
    {
        if (PV.IsMine)
        {
            target = characterGroup.transform.Find(targetName).gameObject;
            targetPos = target.transform.position;
            isCastingSkill = false;
            try
            {
                StopCoroutine(currentCastingSkillCoroutine);                
            }
            catch { }
            try
            {
                StopCoroutine(runWhileCastingSkillCoroutine);
            }
            catch { }
            StartCoroutine(aggroTimer(aggroTime));
        }
    }

    IEnumerator aggroTimer(float time)
    {
        isAggro = true;
        float _time = 0f;
        while(_time < time)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        isAggro = false;
    }


    private void findTarget()
    {
        int numCharacter = characterGroup.transform.childCount;
        if (numCharacter > 0)
        {
            int who = Random.Range(0, numCharacter);
            target = characterGroup.transform.GetChild(who).gameObject;
            targetPos = target.transform.position;
        }
        else
        {
            target = null;
            targetPos = Vector3.zero;
        }
    }
    private void findRandomPosition()
    {
        float x = Random.Range(topLeft.position.x, bottomRight.position.x);
        float y = Random.Range(bottomRight.position.y, topLeft.position.y);
        target = null;
        targetPos = new Vector3(x, y, 0);
    }

    private IEnumerator MoveToTargetCoroutine(Vector3 startPosition, Vector3 targetPosition, float speed, bool run = false, float distance = 2.0f)
    {
        //StopAllCoroutines();         
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        if (run)
            animator.SetBool("run", true);
        while ((transform.position - targetPosition).magnitude > distance)
        {
            while (!isMovable)
            {
                yield return null;
            }


            Vector3 _dirMVec = (targetPosition - transform.position).normalized;
            PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
            float distanceCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            yield return null;
        }
        if (run)
            animator.SetBool("run", false);
    }

    void MoveToTarget(float minDistance, float maxDistance)
    {
        Vector3 _dirVec = target.transform.position - transform.position;
        Vector3 _disVec = (Vector2)target.transform.position - (Vector2)transform.position;

        if (!isMovable)
        {
            return;
        }

        if (_disVec.sqrMagnitude < minDistance && isMoving == true)
        {
            if (monsterSpec.haveWalkMotion)
                animator.SetBool("walk", false);
            isMoving = false;            
            return;
        }
        else if (_disVec.sqrMagnitude > maxDistance && isMoving == false)
        {            
            isMoving = true;
            if (monsterSpec.haveWalkMotion)
                animator.SetBool("walk", true);
        }
        if (isMoving == true)
        {            
            Vector3 _dirMVec = _dirVec.normalized;
            PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
            transform.position += (_dirMVec * monsterSpec.defaultMoveSpeed * Time.deltaTime);
        }
    }


    public void Death()
    {
        attackable = false;
        isDeath = true;
        animator.SetTrigger("Death");
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (string itemName in monsterSpec.dropItems.Keys)
            {
                spawnItem(itemName, monsterSpec.dropItems[itemName]);
            }            
        }
        Destroy(gameObject, 0.45f);
    }

    void spawnItem(string itemName, float prob)
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-0.3f, 0.3f);
        Vector3 spawn_pos = new Vector3(x + transform.position.x, y + transform.position.y, 0);
        float rnd = Random.Range(0, 100f);        
        if (rnd < prob)
        {
            GameObject new_item = PhotonNetwork.InstantiateRoomObject("items/item_prefab", spawn_pos, Quaternion.identity);
            new_item.GetComponent<PhotonView>().RPC("initItem", RpcTarget.All, itemName, 1);
        }
    }

    public void Bind(float bindTime)
    {
        if (!isDeath)
            StartCoroutine(ExcuteBind(bindTime));
    }    
    IEnumerator ExcuteBind(float bindTime)
    {
        attackable = false;
        animator.SetBool("isBound", true);
        animator.SetTrigger("hit");
        float _time = 0f;
        while (_time < bindTime)
        {
            _time += Time.deltaTime;
            yield return null;
        }
        animator.SetBool("isBound", false);
        attackable = true;
    }
    public void Hit()
    {
        if (!isDeath)
        {
            animator.SetTrigger("hit");
            if (monsterSpec.stopWhileHit)
            {
                StopAllCoroutines();
                StartCoroutine(stopWhileHit());
            }
        }
    }

    IEnumerator stopWhileHit()
    {
        isMovable = false;
        float timer_ = 0f;
        while (timer_ < 0.3f)
        {
            timer_ += Time.deltaTime;
            yield return null;
        }
        isMovable = true;
        if (isCastingSkill)
        {
            time = patternCycle;
            isCastingSkill = false;
        }
    }


    [PunRPC]
    void direction(Vector3 _dirMVec)
    {
        if (_dirMVec.x > 0)
        {
            transform.localScale = new Vector3(monsterSpec.defaultScale.x, monsterSpec.defaultScale.y, 1);
            canvas.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (_dirMVec.x < 0)
        {
            transform.localScale = new Vector3(-monsterSpec.defaultScale.x, monsterSpec.defaultScale.y, 1);
            canvas.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void OnDestroy()
    {
        if (monsterSpec.monsterType.ToLower() == "boss")
        {
            gameManager.StageClear(true);
        }
        else if (transform.parent.childCount == 1)
        {
            gameManager.StageClear(false);
        }
    }
}
