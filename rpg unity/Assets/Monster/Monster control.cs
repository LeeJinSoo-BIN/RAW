using Photon.Pun;
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
    private float time = 0f;
    private bool isMoving = false;
    private bool isCastingSkill = false;
    private float patternCycle;
    private MonsterSkillSpec currentCastingSkill;

    private GameObject target;
    private Vector3 targetPos;

    public PhotonView PV;
    public Animator animator;

    
    private void Awake()
    {
        topLeft = GameObject.Find("ground").transform.Find("top left");
        bottomRight = GameObject.Find("ground").transform.Find("bottom right");
        characterGroup = GameObject.Find("Player Group");
        transform.parent = GameObject.Find("Enemy Group").transform;

        patternCycle = monsterSpec.patternCycle;
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && attackable && !isDeath)
        {
            time += Time.deltaTime;

            if (time >= patternCycle && !isCastingSkill) // 30???? ???? ?? ???? ????
            {
                randomPattern();
                time = 0;
            }
            else if (!isCastingSkill)
            {
                if (target != null && monsterSpec.defaultMovement == "following")
                    MoveToTarget();
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
            StartCoroutine(moveRandom());
        else if (currentCastingSkill.type == "Follow")
            StartCoroutine(follow());
        else if (currentCastingSkill.type == "FindTarget")
            findTarget();
        else
            StartCoroutine(castSKill());
    }

    IEnumerator moveRandom()
    {
        isCastingSkill = true;
        findRandomPosition();
        yield return StartCoroutine(MoveToTargetCoroutine(transform.position, targetPos, currentCastingSkill.speed, currentCastingSkill.run));
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
            MoveToTarget();
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

        yield return StartCoroutine(MoveToTargetCoroutine(transform.position, currentTargetPos, currentMoveSpeed, currentCastingSkill.run, currentCastingSkill.distance));
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
        Vector3 _dirMVec = (target.transform.position - transform.position).normalized;
        PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
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
        GameObject spawnObject = PhotonNetwork.Instantiate(currentCastingSkill.skillDirectory, spawnPos, Quaternion.identity);
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
        GameObject area = transform.GetChild(currentCastingSkill.areaChildNum).gameObject;
        area.name = ((int)(currentCastingSkill.flatDeal[0] + currentCastingSkill.increaseDealPerLevel[0] * level)).ToString();
        area.SetActive(true);
        float _time = 0f;
        while (true)
        {
            if (_time > currentCastingSkill.skillDuration)
                break;
            _time += Time.deltaTime;
            yield return null;
        }
        area.SetActive(false);        
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

    void MoveToTarget()
    {
        Vector3 _dirVec = target.transform.position - transform.position;
        Vector3 _disVec = (Vector2)target.transform.position - (Vector2)transform.position;
        if (_disVec.sqrMagnitude < 2f && isMoving == true)
        {
            isMoving = false;
            //animator.SetBool("run", false);
            return;
        }
        else if (_disVec.sqrMagnitude > 5f && isMoving == false)
        {
            isMoving = true;
        }
        if (isMoving == true)
        {
            Vector3 _dirMVec = _dirVec.normalized;
            PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
            transform.position += (_dirMVec * 1f * Time.deltaTime);
        }
    }


    public void Death()
    {
        attackable = false;
        isDeath = true;
        animator.SetTrigger("Death");
        foreach (string itemName in monsterSpec.dropItems.Keys) 
        {
            spawnItem(itemName, monsterSpec.dropItems[itemName]);
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
            GameObject new_item = PhotonNetwork.Instantiate("items/item_prefab", spawn_pos, Quaternion.identity);
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
            animator.SetTrigger("hit");
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
}
