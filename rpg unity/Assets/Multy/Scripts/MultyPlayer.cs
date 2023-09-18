using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class MultyPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool movable = true;
    public bool attackable = true;
    public bool isDeath = false;


    public GameObject movePointer;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public LayerMask monsterLayer;

    public Vector3 goalPos;
    public float pointSpeed = 1.0f;
    public float characterMoveSpeed = 1.0f;
    public Animator characterAnimator;
    public GameObject inventoryUi;
    private GameObject itemBox;
    private GameObject gettingItem;  
    private Dictionary<string, int> carringItems = new Dictionary<string, int>();
    public int maxInventoryCnt = 24;
    
    public GameObject skillRadiusArea;
    public GameObject skillRadiusLengthPoint;
    public GameObject skillRangeAreaCircle;
    public GameObject skillRangeAreaBar;
    public GameObject skillRangeAreaTargeting;
    public Transform skillCastingPosition;

    private GameObject playerGroup;
    private GameObject enemyGroup;

    private bool isActivingSkill = false;
    private string current_casting_skill_key;
    private Vector2 oriSkillRangeAreaBar;
    private IEnumerator castSkill;
    
    private List<string> skill_key = new List<string> { "Q", "W", "E", "R", "A" };    
    private Dictionary<string, SkillSpec> keyToSkillSpec = new Dictionary<string, SkillSpec>();
    private SkillSpec current_skill;
    public CharacterState characterState;    
    private Dictionary<string, float> skillActivatedTime = new Dictionary<string, float>();
    private Dictionary<string, string> skillNameToKey = new Dictionary<string, string>();
    public InGameUI inGameUI;
    private GameObject itemDropField;
    // Multy
    public Rigidbody2D RB;
    public PhotonView PV;
    public Text NickNameText;
    public Canvas canvas;
    public SortingGroup sortingGroup;
    private string skillResourceDir = "Character\\skills";
    Vector3 curPos;
    

    private void Awake()
    {        
        deactivateSkill();
        

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        //sortingGroup.sortingOrder = PV.IsMine ? 1 : 0;

        playerGroup = GameObject.Find("Player Group");
        enemyGroup = GameObject.Find("Enemy Group");
        inGameUI = GameObject.Find("InGameUI").transform.GetChild(0).GetComponent<InGameUI>();
        inGameUI.skillNameToKey = skillNameToKey;
        inventoryUi = GameObject.Find("InGameUI").transform.GetChild(0).GetChild(3).gameObject;
        itemBox = inventoryUi.transform.GetChild(0).GetChild(2).gameObject;
        gettingItem = inventoryUi.transform.GetChild(1).gameObject;
        inGameUI.carringItems = carringItems;
        itemDropField = GameObject.Find("Item Field").gameObject;

        transform.parent = playerGroup.transform;
        
    }
    public void loadData()
    {
        List<string> skill_name_list = characterState.characterSpec.skillLevel.SD_Keys;        
        for (int i = 0; i < characterState.characterSpec.skillLevel.Count; i++)
        {
            Debug.Log(skill_name_list[i]);
            keyToSkillSpec.Add(skill_key[i], GameManager.Instance.skillInfoDict[skill_name_list[i]]);
            skillActivatedTime.Add(skill_name_list[i], 0f);
            skillNameToKey.Add(skill_name_list[i], skill_key[i]);
        }
    }
    void Update()
    {
        if (PV.IsMine && !isDeath)
        {
            if (movable)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);
                    if (hit.collider == null || hit.transform.CompareTag("Not Ground"))
                        return;
                    goalPos = hit.point;
                    StartCoroutine(pointingGoal(goalPos));
                    if (isActivingSkill)
                        deactivateSkill();
                    characterAnimator.SetBool("IsRunning", true);
                }
                Move_Character();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                inventoryUi.SetActive(!inventoryUi.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                goalPos = transform.position;
                try
                {
                    StopCoroutine(castSkill);
                }
                catch { }
                deactivateSkill();
            }
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);                
                if (hit.collider == null || hit.transform.CompareTag("Not Ground"))
                    return;
                if (hit.collider.CompareTag("Item"))
                {
                    if (hit.transform.GetChild(1).gameObject.activeSelf)
                        getItem(hit.transform.gameObject);
                }
                else if (isActivingSkill && attackable)
                {                    
                    if (current_skill.castType == "circle" || current_skill.castType == "bar")
                    { // when cast type is circle or bar
                        RaycastHit2D hit_ground = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);
                        if (hit_ground.transform.CompareTag("Not Ground") || hit_ground.collider == null)
                            return;
                        if (current_skill.castType == "circle")
                            CastingSkill(hit_ground.point);
                        else if (current_skill.castType == "bar")
                            CastingSkill(skillRangeAreaBar.transform.GetChild(1).transform.position);
                    }
                    else if (current_skill.castType == "target-player") // targeting only character 
                    {
                        RaycastHit2D hit_target = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, playerLayer);
                        if (hit_target.collider == null)
                            return;
                        if (skillRangeAreaTargeting.transform.GetChild(1).gameObject.activeSelf)
                            CastingSkill(hit_target.point, hit_target.transform.gameObject);
                    }
                    else if (current_skill.castType == "target-enemy") // targeting only monster
                    {
                        RaycastHit2D hit_target = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, monsterLayer);
                        if (hit_target.collider == null)
                            return;
                        if (skillRangeAreaTargeting.transform.GetChild(1).gameObject.activeSelf)
                            CastingSkill(hit_target.point, hit_target.transform.gameObject);
                    }
                    else if (current_skill.castType == "target-both") // targeting both player and enemy
                    {
                        LayerMask player_or_monster = (playerLayer | monsterLayer);
                        RaycastHit2D hit_target = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, player_or_monster);
                        if (hit_target.collider == null)
                            return;
                        if (skillRangeAreaTargeting.transform.GetChild(1).gameObject.activeSelf)
                            CastingSkill(hit_target.point, hit_target.transform.gameObject);
                    }
                }
            }
            if (attackable)
            {
                /*if (Input.GetKeyDown(KeyCode.A))
                {
                    int attack = Random.Range(1, 4);
                    characterAnimator.SetTrigger("attack" + attack.ToString());
                    goalPos = transform.position;
                }*/
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.A))
                {
                    string now_input_key = Input.inputString.ToUpper();
                    if (now_input_key == current_casting_skill_key)
                        deactivateSkill();
                    else
                    {
                        deactivateSkill();
                        activateSkill(now_input_key);
                    }

                }
            }
            if (isActivingSkill)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                mousePos = new Vector3(mousePos.x, mousePos.y, -1);


                if (current_skill.castType == "circle") // circle
                {
                    skillRangeAreaCircle.transform.position = mousePos;
                }
                else if (current_skill.castType == "bar") // bar
                {
                    //Vector2 target = skillRangeAreaBar.transform.position;
                    Vector2 target = transform.position;
                    float angle_pi = Mathf.Atan2(mousePos.y - target.y, mousePos.x - target.x);
                    float angle_rad = angle_pi * Mathf.Rad2Deg;

                    if (transform.localScale.x > 0)
                        angle_rad -= 180;
                    skillRangeAreaBar.transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);

                    //with cosine equation
                    //float ratio = (float)(Mathf.Cos(2 * angle_pi) / 4 + 0.75);

                    /*
                     * with two dim equation
                    angle_pi = Mathf.Abs(angle_pi) / Mathf.PI;
                    float ratio = 2 * angle_pi * angle_pi - 2 * angle_pi + 1;

                    */

                    //with ellipse equation
                    float a = 1f; // long axis
                    float b = 0.5f; //short axis
                    float slope = (mousePos.y - target.y) / (mousePos.x - target.x);
                    float t = Mathf.Atan((slope * a) / b);
                    float x_intersect = target.x + a * Mathf.Cos(t);
                    float y_intersect = target.y + b * Mathf.Sin(t);
                    float ratio = Mathf.Sqrt((x_intersect - target.x) * (x_intersect - target.x) + (y_intersect - target.y) * (y_intersect - target.y));


                    float scaled_x = oriSkillRangeAreaBar.x * ratio;

                    skillRangeAreaBar.transform.localScale = new Vector2(scaled_x, oriSkillRangeAreaBar.y);
                }
                else if (current_skill.castType == "target-player" || current_skill.castType == "target-enemy" || current_skill.castType == "target-both") //targeting only character
                {
                    skillRangeAreaTargeting.transform.position = mousePos;
                    Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    LayerMask mask;
                    if (current_skill.castType == "target-player")
                        mask = playerLayer;
                    else if (current_skill.castType == "target-enemy")
                        mask = monsterLayer;
                    else
                        mask = playerLayer | monsterLayer;
                    RaycastHit2D hit_object = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, mask);
                    if (hit_object.collider != null)
                        skillRangeAreaTargeting.transform.GetChild(1).gameObject.SetActive(true);
                    else
                        skillRangeAreaTargeting.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100 && !isDeath)
            transform.position = curPos;
        else if(!isDeath)
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }
    void deactivateSkill()
    {
        skillRadiusArea.SetActive(false);
        skillRangeAreaCircle.SetActive(false);
        skillRangeAreaBar.SetActive(false);
        skillRangeAreaTargeting.SetActive(false);
        current_casting_skill_key = "";
        isActivingSkill = false;
        //StopCoroutine(castSkill);        
    }
    void activateSkill(string now_skill_key)
    {
        current_casting_skill_key = now_skill_key;
        current_skill = keyToSkillSpec[now_skill_key];
        
        Vector2 range_area = current_skill.range;

        Vector2 radius_area = current_skill.radius;
        skillRadiusArea.transform.localScale = radius_area;
        skillRadiusArea.SetActive(true);


        isActivingSkill = true;

        if (current_skill.castType == "circle")
        {
            skillRangeAreaCircle.transform.localScale = range_area;
            skillRangeAreaCircle.SetActive(true);
        }
        else if (current_skill.castType == "bar")
        {
            skillRangeAreaBar.transform.localScale = range_area;
            oriSkillRangeAreaBar = range_area;
            skillRangeAreaBar.SetActive(true);
        }
        else if (current_skill.castType == "target-player" || current_skill.castType == "target-enemy" || current_skill.castType == "target-both") // target
        {
            skillRangeAreaTargeting.SetActive(true);
        }
        else if (current_skill.castType == "buff-self" || current_skill.castType == "buff-player" || current_skill.castType == "buff-enemy") // buff
        {
            if (current_skill.castType == "buff-self") // self
                CastingSkill(transform.position, gameObject);
            else if (current_skill.castType == "buff-player") // player
            {
                CastingSkill(transform.position, playerGroup);
            }
            else // enemy
                CastingSkill(transform.position, enemyGroup);
        }
    }
    bool isCoolDown()
    {
        if (skillActivatedTime[current_skill.skillName] == 0)
            return false;
        if (Time.time - skillActivatedTime[current_skill.skillName] < current_skill.coolDown)
            return true;
        return false;
    }
    void CastingSkill(Vector2 skillPos, GameObject target = null)
    {
        Debug.Log("casting skill");
        if (isCoolDown())
        {
            Debug.Log("쿨타임");
            return;
        }
        if (characterState.mana.value < current_skill.consumeMana)
        {
            Debug.Log("마나 부족");
            return;
        }        
        castSkill = CastSkill(skillPos, target);
        StartCoroutine(castSkill);
        if(!current_skill.skillName.Contains("normal"))
            deactivateSkill();
    }

    IEnumerator CastSkill(Vector2 skillPos, GameObject targetObject = null)
    {
        float skill_radius_len = Vector2.Distance(skillRadiusLengthPoint.transform.position, transform.position);

        /*
        float angle_pi = Mathf.Atan2(skillPos.y - transform.position.y, skillPos.x - transform.position.x);
        angle_pi = Mathf.Abs(angle_pi) / Mathf.PI;
        float ratio = 2 * angle_pi * angle_pi - 2 * angle_pi + 1;
        */


        float a = 1f; // long axis
        float b = 0.5f; //short axis
        Vector2 target = transform.position;
        float slope = (skillPos.y - target.y) / (skillPos.x - target.x);
        float t = Mathf.Atan((slope * a) / b);
        float x_intersect = target.x + a * Mathf.Cos(t);
        float y_intersect = target.y + b * Mathf.Sin(t);
        float ratio = Mathf.Sqrt((x_intersect - target.x) * (x_intersect - target.x) + (y_intersect - target.y) * (y_intersect - target.y));

        skill_radius_len *= ratio;

        if (current_skill.castType == "circle" || current_skill.castType == "target-player" || current_skill.castType == "target-enemy" || current_skill.castType == "target-both")
        {
            goalPos = skillPos;
            characterAnimator.SetBool("IsRunning", true);
            Move_Character();
            while (true)
            {
                float skill_casting_point_len = Vector2.Distance(skillPos, transform.position);
                if (skill_radius_len >= skill_casting_point_len)
                {
                    characterAnimator.SetBool("IsRunning", false);
                    goalPos = transform.position;
                    break;
                }
                yield return null;
            }
        }
        else if (current_skill.castType == "bar")
        {
            Vector3 _dirMVec = ((Vector3)skillPos - transform.position).normalized;            
            PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
        }

        goalPos = transform.position;
        movable = false;
        attackable = false;
        characterAnimator.SetBool("IsRunning", false);
        if (current_skill.animType == "normal")
        {
            int attack = Random.Range(1, 4);
            characterAnimator.SetTrigger("attack" + attack.ToString());
        }
        else
            characterAnimator.SetTrigger(current_skill.animType);
        GameObject skill = null;        
        float current_skill_deal = CaculateCharacterSkillDamage(characterState.characterSpec.skillLevel[current_skill.skillName], characterState.characterSpec.power,
            current_skill.flatDeal, current_skill.dealIncreasePerSkillLevel, current_skill.dealIncreasePerPower,
            characterState.characterSpec.criticalPercent, characterState.characterSpec.criticalDamage, affectedByCritical:true);
        float current_skill_heal = CaculateCharacterSkillDamage(characterState.characterSpec.skillLevel[current_skill.skillName], characterState.characterSpec.power,
            current_skill.flatHeal, current_skill.dealIncreasePerSkillLevel, current_skill.dealIncreasePerPower);
        float current_skill_shield = CaculateCharacterSkillDamage(characterState.characterSpec.skillLevel[current_skill.skillName], characterState.characterSpec.power,
            current_skill.flatShield, current_skill.dealIncreasePerSkillLevel, current_skill.dealIncreasePerPower);
        float current_skill_power = CaculateCharacterSkillDamage(characterState.characterSpec.skillLevel[current_skill.skillName], characterState.characterSpec.power,
            current_skill.flatPower, current_skill.powerIncreasePerSkillLevel, current_skill.powerIncreasePerPower);
        Vector2 current_skill_target_pos = default(Vector2);
        string current_skill_target_name = name;
        if (current_skill.castType == "circle")
        {
            skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), skillPos, Quaternion.identity);
        }
        else if (current_skill.castType == "bar")
        {
            skillPos += new Vector2(0f, 0.3f);
            current_skill_target_pos = skillPos;
            if (current_skill.dealType == "throw")
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), skillCastingPosition.position, Quaternion.identity);
            else
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), skillPos, Quaternion.identity);
        }
        else if (current_skill.castType == "target-player" || current_skill.castType == "target-enemy" || current_skill.castType == "target-both") // target
        {
            if (current_skill.dealType == "throw")
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), skillCastingPosition.position, Quaternion.identity);
            else
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), targetObject.transform.position, Quaternion.identity);

            current_skill_target_name = targetObject.name;
        }
        else if (current_skill.castType == "buff-self")
        {
            skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), Vector3.zero, Quaternion.identity);
            current_skill_target_name = gameObject.name;
        }
        else if (current_skill.castType == "buff-player" || current_skill.castType == "buff-enemy") // buff
        {
            foreach (Transform tar in targetObject.GetComponentInChildren<Transform>())
            {
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, current_skill.skillName), tar.transform.position, Quaternion.identity);
                current_skill_target_name = tar.name;
                skill.GetComponent<PhotonView>().RPC("initSkill", RpcTarget.All, current_skill_deal, current_skill_heal, current_skill_shield, current_skill_power, current_skill.duration, current_skill_target_name, current_skill_target_pos);
            }
        }
        if(skill != null)
        {            
            skill.GetComponent<PhotonView>().RPC("initSkill", RpcTarget.All, current_skill_deal, current_skill_heal, current_skill_shield, current_skill_power, current_skill.duration, current_skill_target_name, current_skill_target_pos);            
        }
        if (!current_skill.skillName.Contains("normal"))
        {
            skillActivatedTime[current_skill.skillName] = Time.time;
            inGameUI.CoolDown(current_skill.skillName, current_skill.coolDown);
        }
        characterState.mana.value -= current_skill.consumeMana;
        
        float delay = 0;
        while (delay < current_skill.delay)
        {
            delay += Time.deltaTime;
            yield return null;
        }
        movable = true;
        attackable = true;        
    }


    void getItem(GameObject got_item)
    {
        string got_item_name = got_item.GetComponent<Item>().itemName;
        int got_item_cnt = got_item.GetComponent<Item>().itemCount;        
        if (carringItems.ContainsKey(got_item_name))
        {
            carringItems[got_item_name] += got_item_cnt;            
        }
        else
        {
            carringItems.Add(got_item_name, got_item_cnt);
        }
        updateInventory();
        PV.RPC("itemDestroySync", RpcTarget.All, got_item.name);
        inGameUI.updateAllQuickSlot();
    }

    public void updateInventory()
    {
        List<string> destroyList = new List<string>();
        foreach (string item in carringItems.Keys)
        {
            Debug.Log(item);
            Transform box = itemBox.transform.Find(item);
            if (box != null)
                box.GetChild(1).GetComponent<TMP_Text>().text = carringItems[item].ToString();
            else
            {
                GameObject new_item = Instantiate(gettingItem);
                new_item.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(GameManager.Instance.itemInfoDict[item].spriteDirectory);
                new_item.transform.GetChild(1).GetComponent<TMP_Text>().text = carringItems[item].ToString();
                new_item.transform.SetParent(itemBox.transform);
                new_item.transform.localScale = Vector3.one;
                new_item.name = item;
                new_item.SetActive(true);
            }

            if (carringItems[item] == 0)
            {
                if (box != null)
                {
                    Destroy(box.gameObject);
                }
                destroyList.Add(item);                
            }
        }
        foreach(string name in destroyList)
        {
            carringItems.Remove(name);
        }
    }
    [PunRPC]
    void itemDestroySync(string itemName)
    {
        Destroy(itemDropField.transform.Find(itemName).gameObject);
    }


    void Move_Character()
    {
        Vector3 _dirVec = goalPos - transform.position;
        Vector3 _disVec = (Vector2)goalPos - (Vector2)transform.position;
        if (_disVec.sqrMagnitude < 0.001f)
        {
            characterAnimator.SetBool("IsRunning", false);
            return;
        }
        Vector3 _dirMVec = _dirVec.normalized;
        PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
        transform.position += (_dirMVec * characterMoveSpeed * Time.deltaTime);
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
    
    IEnumerator pointingGoal(Vector2 goalPos)
    {
        GameObject new_move_pointer = Instantiate(movePointer);
        new_move_pointer.gameObject.SetActive(true);
        Vector3 pointer_pos = new Vector3(goalPos.x, goalPos.y, -2f);
        new_move_pointer.transform.position = pointer_pos;
        new_move_pointer.transform.localScale = Vector2.zero;
        while (new_move_pointer.transform.localScale.x < 1f)
        {
            float now_scale_x = new_move_pointer.transform.localScale.x;
            float now_scale_y = new_move_pointer.transform.localScale.x;
            now_scale_x += pointSpeed * Time.deltaTime;
            now_scale_y += pointSpeed * Time.deltaTime;
            new_move_pointer.transform.localScale = new Vector2(now_scale_x, now_scale_y);
            yield return null;
        }
        while (new_move_pointer.transform.localScale.x >= 0)
        {
            float now_scale_x = new_move_pointer.transform.localScale.x;
            float now_scale_y = new_move_pointer.transform.localScale.x;
            now_scale_x -= pointSpeed * Time.deltaTime;
            now_scale_y -= pointSpeed * Time.deltaTime;
            new_move_pointer.transform.localScale = new Vector2(now_scale_x, now_scale_y);
            yield return null;
        }
        Destroy(new_move_pointer);
    }

    float CaculateCharacterSkillDamage(float skillLevel, float casterPower, float flat, float perSkillLevel, float perPower, float criticalPercent = 0f, float criticalDamage = 1f, bool affectedByCritical = false)
    {
        float value = flat + perSkillLevel * skillLevel
            + perPower * casterPower;
        if (affectedByCritical) // damage
        {
            float crit = Random.Range(0f, 100f);

            float critical_damage = criticalDamage;
            if (crit < criticalPercent)
                critical_damage = 1;
            value *= critical_damage;
        }
        return value;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
        }
    }
}
