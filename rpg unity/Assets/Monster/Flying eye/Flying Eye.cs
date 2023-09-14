using Photon.Pun;
using System.Collections;
using UnityEngine;


public class FlyingEye : MonoBehaviour
{
    private Transform topLeft;
    private Transform bottomRight;
    public GameObject characterGroup;

    public Animator animator;
    public GameObject skill2Area;
    public GameObject skill3Area;

    private float _time = 0.0f;
    private bool skillInvoked = false;

    private GameObject target;
    private Vector3 targetPos;

    public PhotonView PV;
    public bool attackable = true;
    public bool isDeath = false;
    private float patternCycle = 2f;
    private bool isMoving = false;
    public GameObject canvas;
    void Awake()
    {
        topLeft = GameObject.Find("ground").transform.Find("top left");
        bottomRight = GameObject.Find("ground").transform.Find("bottom right");
        characterGroup = GameObject.Find("Player Group");
        transform.parent = GameObject.Find("Enemy Group").transform;
        name = name + transform.parent.childCount.ToString();
        transform.localScale = Vector3.one;
        findTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && attackable && !isDeath)
        {
            _time += Time.deltaTime;

            if (_time >= patternCycle && skillInvoked == false) // 30???? ???? ?? ???? ????
            {
                randomPattern();
                _time = 0;
            }
            else if (skillInvoked == false)
            {
                if (target != null)
                    MoveToTarget();
            }
            if (skillInvoked)
                _time = 0;
        }
    }

    public void Death()
    {
        attackable = false;
        isDeath = true;
        animator.SetTrigger("Death");
        spawnItem();
        Destroy(gameObject, 0.45f);
    }

    void spawnItem()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-0.3f, 0.3f);
        Vector3 spawn_pos = new Vector3(x + transform.position.x, y + transform.position.y, 0);

        int rnd = Random.Range(0, 3);
        if (rnd == 0)
        {
            GameObject new_item = PhotonNetwork.Instantiate("items/item_prefab", spawn_pos, Quaternion.identity);
            new_item.GetComponent<PhotonView>().RPC("initItem", RpcTarget.All, "red potion small", 1);
        }
        else if (rnd == 1)
        {
            GameObject new_item = PhotonNetwork.Instantiate("items/item_prefab", spawn_pos, Quaternion.identity);
            new_item.GetComponent<PhotonView>().RPC("initItem", RpcTarget.All, "blue potion small", 1);
        }
    }

    public void Bind(float bindTime = 5f)
    {
        if (!isDeath)
            StartCoroutine(ExcuteBind(bindTime));
    }
    public void Hit()
    {
        if (!isDeath)
            animator.SetTrigger("hit");
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
            /*if (targetPosition.x < transform.position.x && transform.localScale.x > 0)
                transform.localScale = new Vector3(-1, 1);
            else if (targetPosition.x > transform.position.x && transform.localScale.x < 0)
                transform.localScale = new Vector3(1, 1);*/
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
    void skill1()
    {
        findTarget();
    }

    void skill2()
    {        
        StartCoroutine(excuteSkill2());
    }
    IEnumerator excuteSkill2()
    {
        skillInvoked = true;
        findTarget();
        yield return StartCoroutine(MoveToTargetCoroutine(transform.position, targetPos, 4f, run:true, distance:0f));
        float _time = 0f;
        animator.SetTrigger("attack2");
        while (true)
        {
            if (_time > 0.3f)
                break;
            _time += Time.deltaTime;
            yield return null;
        }
        skill2Area.SetActive(true);
        _time = 0f;
        while (true)
        {
            if (_time > 0.3f)
                break;
            _time += Time.deltaTime;
            yield return null;
        }
        skill2Area.SetActive(false);
        skillInvoked = false;
    }

    void skill3()
    {        
        StartCoroutine(excuteSkill3());
    }
    IEnumerator excuteSkill3()
    {
        skillInvoked = true;
        findTarget();
        yield return StartCoroutine(MoveToTargetCoroutine(transform.position, targetPos, 3f, run: true, distance: 0f));
        float _time = 0f;
        animator.SetTrigger("attack3");
        while (true)
        {
            if (_time > 0.2f)
                break;
            _time += Time.deltaTime;
            yield return null;
        }
        skill3Area.SetActive(true);
        _time = 0f;
        while (true)
        {
            if (_time > 0.3f)
                break;
            _time += Time.deltaTime;
            yield return null;
        }
        skill3Area.SetActive(false);
        skillInvoked = false;
    }
    void randomPattern()
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
                skill3();
                break;
        }
    }

    [PunRPC]
    void direction(Vector3 _dirMVec)
    {
        if (_dirMVec.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            canvas.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (_dirMVec.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            canvas.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
