using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Linq;
using System;
using WebSocketSharp;

public class MultyPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool movable = true;
    public bool attackable = true;
    public bool isDeath = false;


    public GameObject movePointer;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public LayerMask monsterLayer;
    public LayerMask itemLayer;
    public LayerMask npcLayer;

    public Vector3 goalPos;
    public float pointSpeed = 1.0f;
    public float characterMoveSpeed = 1.0f;
    public Animator characterAnimator;
    private int frontInventoryPos = 0;
    public Dictionary<string, QuickInventory> quickInventory = new Dictionary<string, QuickInventory>();
    
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
    public bool isCastingSkill;
    
    private List<string> skill_key = new List<string> { "q", "w", "e", "r", "a" };
    private SkillSpec current_skill;
    private bool isAttackingNormal;
    
    private GameObject normalAttackTarget;
    private SkillSpec normalAttackSpec;

    public CharacterState characterState;
    private CharacterSpec characterSpec;
    private Dictionary<string, float> skillActivatedTime = new Dictionary<string, float>();
    public GameObject itemDropField;

    private EventSystem eventSystem;
    // Multy
    public Rigidbody2D RB;
    public PhotonView PV;
    public TMP_Text NickNameText;
    public Canvas canvas;
    public SortingGroup sortingGroup;
    private string skillResourceDir = "Character\\skills";
    Vector3 curPos;
    

    private void Awake()
    {        
        deactivateSkill();
        

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        if (PV.Owner.NickName.ToLower() == "binary01")
            NickNameText.text = "<color=red>B</color><color=orange>I</color><color=yellow>N</color><color=green>A</color><color=blue>R</color><color=purple>Y</color><color=black>0</color><color=white>1</color>";
        else
            NickNameText.color = PV.IsMine ? Color.green : Color.red;

        playerGroup = GameObject.Find("Player Group");
        enemyGroup = GameObject.Find("Enemy Group");
        itemDropField = GameObject.Find("Item Field").gameObject;
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        //sortingGroup.sortingOrder = PV.IsMine ? 1 : 0;        
               
        transform.parent = playerGroup.transform;
    }
    public void loadData()
    {
        characterSpec = characterState.characterSpec;
        PV.RPC("setName", RpcTarget.AllBuffered, characterSpec.nickName + PV.ViewID.ToString());
        foreach(string key in skill_key)
        {
            string skillName = characterSpec.skillQuickSlot[key];
            skillActivatedTime.Add(skillName, 0f);
            if (key == "a")
                normalAttackSpec = DataBase.Instance.skillInfoDict[skillName];
        }


        quickInventory.Clear();
        for (int k = 0; k < characterSpec.maxInventoryNum; k++)
        {
            InventoryItem item = characterSpec.inventory[k];
            if (item == null) continue;
            if (!quickInventory.ContainsKey(item.itemName))
            {
                quickInventory.Add(item.itemName, new QuickInventory { kindCount = item.count, position = new SortedSet<int> { k } });
            }
            else
            {
                quickInventory[item.itemName].kindCount += item.count;
                quickInventory[item.itemName].position.Add(k);
            }
        }        
        UIManager.Instance.quickInventory = quickInventory;
        UIManager.Instance.updateInventory();
    }

    void Update()
    {        
        if (PV.IsMine && !isDeath)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject() == false)
                {
                    Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit_item = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, itemLayer);
                    if (hit_item.collider != null)
                    {
                        //Debug.Log(hit_item.collider.name);
                        if (hit_item.collider.CompareTag("Item"))
                        {
                            //Debug.Log("click item");
                            if (hit_item.transform.GetChild(1).gameObject.activeSelf)
                            {
                                Item pickingItem = hit_item.transform.GetComponent<Item>();
                                if (!pickingItem.isSomeonePicking)
                                    pickingItem.PV.RPC("pickItem", RpcTarget.All, true);
                                getItem(hit_item.transform.GetComponent<Item>(), true);
                            }
                        }
                        return;
                    }
                    if (isActivingSkill && attackable)
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
                                CastingSkill(hit_target.transform.Find("foot").position, hit_target.transform.gameObject);
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
                        return;
                    }

                    RaycastHit2D hit_npc = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, npcLayer);
                    if (hit_npc.collider != null)
                    {
                        if (hit_npc.collider.CompareTag("NPC"))
                        {
                            Debug.Log(hit_npc.collider.name);
                            hit_npc.transform.GetComponent<NPC>().ClickNPC();
                        }
                    }
                }            
            }
            if (movable)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);
                    if (hit.collider == null || hit.transform.CompareTag("Not Ground"))
                        return;
                    goalPos = hit.point;
                    isAttackingNormal = false;
                    normalAttackTarget = null;
                    stopCastingSkill();
                    StartCoroutine(pointingGoal(goalPos));
                    if (isActivingSkill)
                        deactivateSkill();
                    characterAnimator.SetBool("IsRunning", true);                    
                }
                Move_Character();
            }            
            if (eventSystem.currentSelectedGameObject == null)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    goalPos = transform.position;
                    isAttackingNormal = false;
                    normalAttackTarget = null;
                    stopCastingSkill();
                    deactivateSkill();
                }
                if (attackable)
                {
                    if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.A))
                    {
                        string now_input_key = Input.inputString.ToLower();
                        if (now_input_key.Length > 1)
                            return;
                        if (characterSpec.skillQuickSlot[now_input_key].IsNullOrEmpty())
                            return;
                        if (now_input_key == current_casting_skill_key)
                            deactivateSkill();
                        else
                        {
                            deactivateSkill();
                            isAttackingNormal = false;
                            activateSkill(now_input_key);
                        }
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
            if(attackable && isAttackingNormal && !isCastingSkill)
            {
                if (normalAttackTarget != null && !isCoolDown(normalAttackSpec))
                {
                    CastingSkill(normalAttackTarget.transform.Find("foot").position, normalAttackTarget);
                }
            }
        }        
        else if ((transform.position - curPos).sqrMagnitude >= 100 && !isDeath)
            transform.position = curPos;
        else if (!isDeath)
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);        
    }
    public void deactivateSkill()
    {
        skillRadiusArea.SetActive(false);
        skillRangeAreaCircle.SetActive(false);
        skillRangeAreaBar.SetActive(false);
        skillRangeAreaTargeting.SetActive(false);
        current_casting_skill_key = "";
        current_skill = null;
        isActivingSkill = false;        
    }
    void activateSkill(string now_skill_key)
    {
        current_casting_skill_key = now_skill_key;
        current_skill = DataBase.Instance.skillInfoDict[characterSpec.skillQuickSlot[now_skill_key]];
        if (isCoolDown(current_skill) && !current_skill.skillName.Contains("normal"))
        {
            return;
        }
        if (characterState.mana.value < current_skill.consumeMana)
        {
            return;
        }
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
    bool isCoolDown(SkillSpec skill)
    {
        if (skill != null)
        {
            if (skillActivatedTime[skill.skillName] == 0)
                return false;
            if (Time.time - skillActivatedTime[skill.skillName] < skill.coolDown)
                return true;
        }
        else
            return true;
        return false;
    }
    void CastingSkill(Vector2 skillPos, GameObject target = null)
    {
        if (isAttackingNormal)
            current_skill = normalAttackSpec;
        
        if (isCoolDown(current_skill))
        {
            print("쿨타임");
            return;
        }
        if (characterState.mana.value < current_skill.consumeMana)
        {
            print("마나 부족");
            return;
        }
        stopCastingSkill();
        if (current_skill.skillName.Contains("normal"))
        {
            isAttackingNormal = true;
            normalAttackTarget = target;
        }
        else
        {
            isAttackingNormal = false;
            normalAttackTarget = null;
        }
        castSkill = CastSkill(skillPos, current_skill, current_casting_skill_key, target);
        StartCoroutine(castSkill);
        //if(!current_skill.skillName.Contains("normal"))
            deactivateSkill();
    }

    IEnumerator CastSkill(Vector2 skillPos, SkillSpec currentCastingSkill, string currentCastingSkillKey, GameObject targetObject = null)
    {
        isCastingSkill = true;
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
        if (currentCastingSkill.castType == "circle" || currentCastingSkill.castType == "target-player" || currentCastingSkill.castType == "target-enemy" || currentCastingSkill.castType == "target-both")
        {
            goalPos = skillPos;
            characterAnimator.SetBool("IsRunning", true);
            Move_Character();
            while (true)
            {
                float skill_casting_point_len = Vector2.Distance(goalPos, transform.position);
                if (currentCastingSkill.castType.Contains("target"))
                {
                    if (targetObject == null)
                    {
                        characterAnimator.SetBool("IsRunning", false);
                        goalPos = targetObject.transform.Find("foot").position;                        
                        isCastingSkill = false;
                        yield break;
                    }
                    goalPos = targetObject.transform.Find("foot").position;
                }
                if (skill_radius_len >= skill_casting_point_len)
                {
                    characterAnimator.SetBool("IsRunning", false);
                    goalPos = transform.position;
                    break;
                }
                yield return null;
            }
        }
        else if (currentCastingSkill.castType == "bar")
        {
            Vector3 _dirMVec = ((Vector3)skillPos - transform.position).normalized;
            PV.RPC("direction", RpcTarget.AllBuffered, _dirMVec);
        }

        goalPos = transform.position;
        movable = false;
        attackable = false;
        characterAnimator.SetBool("IsRunning", false);
        if (currentCastingSkill.animType == "normal")
        {
            int attack = UnityEngine.Random.Range(1, 4);
            characterAnimator.SetTrigger("attack" + attack.ToString());
        }
        else
            characterAnimator.SetTrigger(currentCastingSkill.animType);
        GameObject skill = null;
        float[] current_skill_deal_ = CaculateCharacterSkillDamage(characterSpec.skillLevel[currentCastingSkill.skillName], characterState.power,
            currentCastingSkill.flatDeal, currentCastingSkill.dealIncreasePerSkillLevel, currentCastingSkill.dealIncreasePerPower,
            characterSpec.criticalPercent, characterSpec.criticalDamage, affectedByCritical: true);
        bool isCritical = false;
        if (current_skill_deal_[1] == 1)
            isCritical = true;
        float current_skill_deal = current_skill_deal_[0];
        float current_skill_heal = CaculateCharacterSkillDamage(characterSpec.skillLevel[currentCastingSkill.skillName], characterState.power,
            currentCastingSkill.flatHeal, currentCastingSkill.healIncreasePerSkillLevel, currentCastingSkill.healIncreasePerPower)[0];
        float current_skill_shield = CaculateCharacterSkillDamage(characterSpec.skillLevel[currentCastingSkill.skillName], characterState.power,
            currentCastingSkill.flatShield, currentCastingSkill.shieldIncreasePerSkillLevel, currentCastingSkill.shieldIncreasePerPower)[0];
        float current_skill_power = CaculateCharacterSkillDamage(characterSpec.skillLevel[currentCastingSkill.skillName], characterState.power,
            currentCastingSkill.flatPower, currentCastingSkill.powerIncreasePerSkillLevel, currentCastingSkill.powerIncreasePerPower)[0];
        Vector2 current_skill_target_pos = default(Vector2);
        string current_skill_target_name = name;
        if (currentCastingSkill.castType == "circle")
        {
            skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), skillPos, Quaternion.identity);
        }
        else if (currentCastingSkill.castType == "bar")
        {
            skillPos += new Vector2(0f, 0.3f);
            current_skill_target_pos = skillPos;
            if (currentCastingSkill.dealType == "throw")
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), skillCastingPosition.position, Quaternion.identity);
            else
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), skillPos, Quaternion.identity);
        }
        else if (currentCastingSkill.castType == "target-player" || currentCastingSkill.castType == "target-enemy" || currentCastingSkill.castType == "target-both") // target
        {
            if (currentCastingSkill.dealType == "throw")
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), skillCastingPosition.position, Quaternion.identity);
            else
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), targetObject.transform.position, Quaternion.identity);

            current_skill_target_name = targetObject.name;
        }
        else if (currentCastingSkill.castType == "buff-self")
        {
            skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), Vector3.zero, Quaternion.identity);
            current_skill_target_name = gameObject.name;
        }
        else if (currentCastingSkill.castType == "buff-player" || currentCastingSkill.castType == "buff-enemy") // buff
        {
            foreach (Transform tar in targetObject.GetComponentInChildren<Transform>())
            {
                skill = PhotonNetwork.Instantiate(Path.Combine(skillResourceDir, currentCastingSkill.skillName), tar.transform.position, Quaternion.identity);
                current_skill_target_name = tar.name;
                skill.GetComponent<PhotonView>().RPC("initSkill", RpcTarget.All, current_skill_deal, current_skill_heal, current_skill_shield, current_skill_power, isCritical, currentCastingSkill.dealSync, currentCastingSkill.duration, current_skill_target_name, current_skill_target_pos);
            }
            skill = null;
        }
        if (skill != null)
            skill.GetComponent<PhotonView>().RPC("initSkill", RpcTarget.All, current_skill_deal, current_skill_heal, current_skill_shield, current_skill_power, isCritical, currentCastingSkill.dealSync, currentCastingSkill.duration, current_skill_target_name, current_skill_target_pos);
        
        skillActivatedTime[currentCastingSkill.skillName] = Time.time;
        UIManager.Instance.CoolDown(currentCastingSkillKey, currentCastingSkill.coolDown);
        characterState.mana.value -= currentCastingSkill.consumeMana;

        float delay = 0;
        while (delay < currentCastingSkill.delay)
        {
            delay += Time.deltaTime;
            yield return null;
        }
        movable = true;
        attackable = true;
        isCastingSkill = false;
    }

    void stopCastingSkill()
    {
        try
        {
            StopCoroutine(castSkill);
        }
        catch
        {

        }
        if (isCastingSkill)
        {
            isCastingSkill = false;
            movable = true;
            attackable = true;
        }
        isAttackingNormal = false;
    }
    public bool getItem(Item got_item, bool pick, int invenPos = -1)
    {
        //string got_item_name = got_item.GetComponent<Item>().itemName;
        //int got_item_cnt = got_item.GetComponent<Item>().itemCount;
        bool gotten = false;

        if (DataBase.Instance.itemInfoDict[got_item.itemName].itemType == "coin")
        {
            DataBase.Instance.selectedCharacterSpec.money += got_item.itemCount;
            gotten = true;
            if (pick)
            {
                got_item.PV.RPC("destroyItem", RpcTarget.All);
            }
            return true;
        }
        if(invenPos != -1)
        {
            if (got_item.itemCount <= DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount)
            {
                if(quickInventory.ContainsKey(got_item.itemName))
                {
                    quickInventory[got_item.itemName].position.Add(invenPos);
                    quickInventory[got_item.itemName].kindCount += got_item.itemCount;
                }
                else
                {
                    quickInventory.Add(got_item.itemName, new QuickInventory { kindCount = got_item.itemCount, position = new SortedSet<int> { invenPos } });
                }
                InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
                newItem.itemName = got_item.itemName;
                newItem.reinforce = got_item.reinforce;
                newItem.count = got_item.itemCount;
                characterSpec.inventory[invenPos] = newItem;

                return true;
            }
            else
            {
                Debug.LogError("cant get that amount. it is over max Carry");
                return false;
            }
        }
        if (quickInventory.ContainsKey(got_item.itemName))
        {
            int findPos = -1;
            foreach (int pos in quickInventory[got_item.itemName].position)
            {
                if (characterSpec.inventory[pos].count == DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount)
                    continue;
                findPos = pos;
                break;
            }
            if (findPos != -1)
            {
                if (characterSpec.inventory[findPos].count + got_item.itemCount <= DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount)
                {
                    characterSpec.inventory[findPos].count += got_item.itemCount;
                    quickInventory[got_item.itemName].kindCount += got_item.itemCount;
                    gotten = true;
                }
                else
                {
                    int remainedCnt = characterSpec.inventory[findPos].count + got_item.itemCount - DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount;
                    characterSpec.inventory[findPos].count += (got_item.itemCount - remainedCnt);
                    quickInventory[got_item.itemName].kindCount += (got_item.itemCount - remainedCnt);
                    got_item.itemCount = remainedCnt;
                    getItem(got_item, pick);
                    return false;
                }
            }
            else
            {

                FindFrontInventoryPos();
                if (frontInventoryPos != -1)
                {
                    if (got_item.itemCount <= DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount)
                    {
                        quickInventory[got_item.itemName].position.Add(frontInventoryPos);
                        quickInventory[got_item.itemName].kindCount += got_item.itemCount;                        

                        InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
                        newItem.itemName = got_item.itemName;
                        newItem.reinforce = got_item.reinforce;
                        newItem.count = got_item.itemCount;
                        characterSpec.inventory[frontInventoryPos] = newItem;
                        gotten = true;
                    }
                    else
                    {
                        int remainedCnt = characterSpec.inventory[frontInventoryPos].count + got_item.itemCount - DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount;
                        quickInventory[got_item.itemName].position.Add(frontInventoryPos);
                        quickInventory[got_item.itemName].kindCount += got_item.itemCount - remainedCnt;                        

                        InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
                        newItem.itemName = got_item.itemName;
                        newItem.reinforce = got_item.reinforce;
                        newItem.count = got_item.itemCount - remainedCnt;
                        characterSpec.inventory[frontInventoryPos] = newItem;

                        got_item.itemCount = remainedCnt;
                        getItem(got_item, pick);
                        return false;
                    }
                }
                else
                {
                    gotten = false;
                }
            }
        }
        else
        {
            FindFrontInventoryPos();
            if (frontInventoryPos != -1)
            {
                if (got_item.itemCount <= DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount)
                {
                    quickInventory.Add(got_item.itemName,new QuickInventory {  kindCount = got_item.itemCount, position = new SortedSet<int> { frontInventoryPos } });
                    InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
                    newItem.itemName = got_item.itemName;
                    newItem.reinforce = got_item.reinforce;
                    newItem.count = got_item.itemCount;
                    characterSpec.inventory[frontInventoryPos] = newItem;
                    gotten = true;
                }
                else
                {
                    int remainedCnt = characterSpec.inventory[frontInventoryPos].count + got_item.itemCount - DataBase.Instance.itemInfoDict[got_item.itemName].maxCarryAmount;
                    quickInventory.Add(got_item.itemName, new QuickInventory { kindCount = got_item.itemCount - remainedCnt, position = new SortedSet<int> { frontInventoryPos } });
                    InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
                    newItem.itemName = got_item.itemName;
                    newItem.reinforce = got_item.reinforce;
                    newItem.count = got_item.itemCount;
                    characterSpec.inventory[frontInventoryPos] = newItem;
                    got_item.itemCount = remainedCnt;
                    getItem(got_item, pick);
                    return false;
                }
            }
            else
            {
                gotten = false;
            }
        }
        if (pick)
        {
            if (gotten)
            {
                got_item.PV.RPC("destroyItem", RpcTarget.All);
            }
            else
            {
                got_item.PV.RPC("initItem", RpcTarget.All, got_item.itemName, got_item.itemCount);
                got_item.PV.RPC("piclItem", RpcTarget.All, false);
            }
        }
        UIManager.Instance.updateInventory();
        UIManager.Instance.updateAllQuickSlot();
        
        return gotten;
    }

    public bool loseItem(string itemName, int cnt, int where = -1)
    {
        if (where != -1)
        {
            if (characterSpec.inventory[where].count < cnt)
                return false;
            characterSpec.inventory[where].count -= cnt;
            quickInventory[itemName].kindCount -= cnt;
            if (characterSpec.inventory[where].count == 0)
            {
                characterSpec.inventory[where] = null;
                quickInventory[itemName].position.Remove(where);                
                if (quickInventory[itemName].kindCount == 0)
                    quickInventory.Remove(itemName);
            }
            UIManager.Instance.updateInventory();
            UIManager.Instance.updateAllQuickSlot();
            return true;
        }
        if (quickInventory[itemName].kindCount < cnt)
            return false;

        List<int> deleteList = new List<int>();
        int[] posList = quickInventory[itemName].position.ToArray();
        Array.Sort(posList, (a, b) => b.CompareTo(a));
        foreach (int pos in posList)
        {
            if (characterSpec.inventory[pos].count > cnt)
            {
                characterSpec.inventory[pos].count -= cnt;
                quickInventory[itemName].kindCount -= cnt;

                break;
            }
            else if (characterSpec.inventory[pos].count == cnt)
            {
                characterSpec.inventory[pos] = null;
                deleteList.Add(pos);
                quickInventory[itemName].kindCount -= cnt;
                break;
            }
            else
            {
                cnt -= characterSpec.inventory[pos].count;
                quickInventory[itemName].kindCount -= characterSpec.inventory[pos].count;
                characterSpec.inventory[pos] = null;
                deleteList.Add(pos);
            }
        }

        foreach (int pos in deleteList)
        {
            quickInventory[itemName].position.Remove(pos);
        }
        if (quickInventory[itemName].kindCount == 0)
            quickInventory.Remove(itemName);
        UIManager.Instance.updateInventory();
        UIManager.Instance.updateAllQuickSlot();
        return true;
    }
    public void FindFrontInventoryPos()
    {
        for (int k = 0; k < DataBase.Instance.selectedCharacterSpec.inventory.Count; k++)
        {
            if (DataBase.Instance.selectedCharacterSpec.inventory[k] == null)
            {
                frontInventoryPos = k;
                return;
            }
        }
        frontInventoryPos = -1;
    }
    /*public void updateInventory()
    {
        List<string> destroyList = new List<string>();
        foreach (string item in quickInventory.Keys)
        {
            int pos = quickInventory[item].position;
            int cnt = quickInventory[item].count;
            Transform box = itemBox.transform.GetChild(pos);
            if (box.GetChild(1).GetComponent<Image>().color.a != 0)
            {
                box.GetChild(2).GetComponent<TMP_Text>().text = cnt.ToString();
                box.GetChild(1).GetComponent<Image>().color = Color.white;
            }
            else
            {
                box.GetChild(2).GetComponent<TMP_Text>().text = cnt.ToString();
                box.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(DataBase.Instance.itemInfoDict[item].spriteDirectory);
                box.GetChild(1).GetComponent<Image>().color = Color.white;                
            }
            if (cnt == 0)
            {
                destroyList.Add(item);                
                box.GetChild(2).GetComponent<TMP_Text>().text = "";
                box.GetChild(1).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
            }
        }
        foreach (string name in destroyList)
        {
            quickInventory.Remove(name);
        }
        *//*for (int k = 0; k < inventory.Count; k++)
        {
            int pos = inventory[k].position;
            int cnt = inventory[k].count;
            Transform current_item_box = itemBox.transform.GetChild(pos);
            if (cnt > 0)
            {
                if (current_item_box.gameObject.activeSelf)
                {
                    current_item_box.GetChild(1).GetComponent<TMP_Text>().text = cnt.ToString();
                }
                else
                {
                    current_item_box.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(GameManager.Instance.itemInfoDict[inventory[k].itemName].spriteDirectory);
                    current_item_box.GetChild(1).GetComponent<TMP_Text>().text = cnt.ToString();
                    current_item_box.name = inventory[k].itemName;
                    current_item_box.gameObject.SetActive(true);
                }
            }
            else
            {
                if (current_item_box.gameObject.activeSelf)
                {
                    current_item_box.gameObject.SetActive(false);
                }
                destroy.Add(k);
            }
        }
        foreach(int index in destroy)
        {
            inventory.RemoveAt(index);
        }*//*
    }*/
    [PunRPC]
    void itemDestroySync(string itemName)
    {
        //Debug.Log(itemName);
        Destroy(itemDropField.transform.Find(itemName).gameObject);
    }

    [PunRPC]
    void pickPartOfItem(string itemName, int cnt)
    {
        itemDropField.transform.Find(itemName).GetComponent<Item>().itemCount = cnt;
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

    float[] CaculateCharacterSkillDamage(float skillLevel, float casterPower, float flat, float perSkillLevel, float perPower, float criticalPercent = 0f, float criticalDamage = 1f, bool affectedByCritical = false)
    {
        float[] value = new float[2];
        value[0] = flat + perSkillLevel * skillLevel
            + perPower * casterPower;
        value[1] = 0;
        if (affectedByCritical) // damage
        {
            float crit = UnityEngine.Random.Range(0f, 100f);

            float critical_damage = criticalDamage;
            if (crit < criticalPercent)
                critical_damage = 1;
            else
                value[1] = 1;
            value[0] *= critical_damage;
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

    [PunRPC]
    void setName(string nickName)
    {
        name = nickName;
    }
}
