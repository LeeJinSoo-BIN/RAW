using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;

public class CharacterControl : MonoBehaviour
{
    // Start is called before the first frame update   

    public bool movable = true;
    public GameObject movePointer;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public LayerMask monsterLayer;

    private Vector3 goalPos;
    public float pointSpeed = 1.0f;
    public float characterMoveSpeed = 1.0f;
    public Animator characterAnimator;
    public GameObject inventoryUi;
    private GameObject itemBox;
    private GameObject gettingItem;
    private Dictionary<string, int> itemsInInventory = new Dictionary<string, int>();
    public int maxInventoryCnt = 24;
    public bool attackable = true;
    public GameObject skillRadiusArea;
    public GameObject skillRadiusLengthPoint;
    public GameObject skillRangeAreaCircle;
    public GameObject skillRangeAreaBar;
    public GameObject skillRangeAreaTargeting;
    public Transform skillCastingPosition;

    public GameObject playerGroup;
    public GameObject enemyGroup;

    private bool isActivingSkill = false;
    private string current_casting_skill_name;
    private string current_casting_skill_key;
    private int current_casting_skill_type;
    private Vector2 oriSkillRangeAreaBar;
    private IEnumerator castSkill;
    
    //arrow  sword  magic
    public string characterRoll;
    void Start()
    {
        itemBox = inventoryUi.transform.GetChild(0).GetChild(2).gameObject;
        gettingItem = inventoryUi.transform.GetChild(1).gameObject;
        deactivateSkill();
    }

    // Update is called once per frame
    void Update()
    {
        if (movable)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);

                if (hit.collider != null)
                {
                    goalPos = hit.point;
                    StartCoroutine(pointingGoal(goalPos));
                    if (isActivingSkill)
                    {
                        deactivateSkill();
                    }
                }
                characterAnimator.SetBool("IsRunning", true);
            }
            Move_Character();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUi.SetActive(!inventoryUi.activeSelf);
        }
        if( Input.GetKeyDown(KeyCode.S))
        {
            goalPos = transform.position;
            StopCoroutine(castSkill);
            deactivateSkill();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Item"))
                {
                    if (hit.transform.GetChild(1).gameObject.activeSelf)
                        getItem(hit.transform.gameObject);
                }
            }
            if (isActivingSkill)
            {
                if (current_casting_skill_type == 0 || current_casting_skill_type == 1)
                { // when cast type is circle or bar
                    RaycastHit2D hit_ground = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);
                    if (hit_ground.collider != null)
                    {
                        if (current_casting_skill_type == 0)
                            CastingSkill(hit_ground.point);
                        else if (current_casting_skill_type == 1)
                            CastingSkill(skillRangeAreaBar.transform.GetChild(1).transform.position);
                    }
                }
                else if(current_casting_skill_type == 2) // targeting only character 
                {
                    //LayerMask player_or_monster = (playerLayer | monsterLayer);
                    RaycastHit2D hit_target = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, playerLayer);
                    if (skillRangeAreaTargeting.transform.GetChild(1).gameObject.activeSelf)
                        CastingSkill(hit_target.point, hit_target.transform.gameObject);
                }
                else if (current_casting_skill_type == 3) // targeting only monster
                {
                    //LayerMask player_or_monster = (playerLayer | monsterLayer);
                    RaycastHit2D hit_target = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, monsterLayer);
                    if (skillRangeAreaTargeting.transform.GetChild(1).gameObject.activeSelf)
                        CastingSkill(hit_target.point, hit_target.transform.gameObject);
                }
                else if (current_casting_skill_type == 4) // targeting both player and enemy
                {
                    LayerMask player_or_monster = (playerLayer | monsterLayer);
                    RaycastHit2D hit_target = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, player_or_monster);
                    if (skillRangeAreaTargeting.transform.GetChild(1).gameObject.activeSelf)
                        CastingSkill(hit_target.point, hit_target.transform.gameObject);
                }
            }
        }
        if (attackable)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                int attack = Random.Range(1, 4);
                characterAnimator.SetTrigger("attack" + attack.ToString());
                goalPos = transform.position;
            }
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
            {

                string now_input_key = Input.inputString.ToUpper();
                if (now_input_key == current_casting_skill_key)
                    deactivateSkill();
                else
                {
                    deactivateSkill();
                    activeSkill(now_input_key);
                }

            }
        }
        if (isActivingSkill)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            mousePos = new Vector3(mousePos.x, mousePos.y, -1);


            if (current_casting_skill_type == 0) // circle
            {
                skillRangeAreaCircle.transform.position = mousePos;
            }
            else if (current_casting_skill_type == 1) // bar
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
            else if (current_casting_skill_type == 2 || current_casting_skill_type == 3 || current_casting_skill_type == 4) //targeting only character
            {
                skillRangeAreaTargeting.transform.position = mousePos;
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                LayerMask mask;
                if (current_casting_skill_type == 2)
                    mask = playerLayer;
                else if (current_casting_skill_type == 3)
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
    void activeSkill(string now_skill_key)
    {
        
        current_casting_skill_key = now_skill_key;        
        current_casting_skill_name = SkillManager.instance.rollSkills[characterRoll][now_skill_key];
        current_casting_skill_type = SkillManager.instance.skillData[current_casting_skill_name].castType;
        (float x, float y) range_xy = SkillManager.instance.skillData[current_casting_skill_name].range;
        Vector2 range_area = new Vector2(range_xy.x, range_xy.y);

        (float x, float y) radius_xy = SkillManager.instance.skillData[current_casting_skill_name].radius;
        Vector2 radius_area = new Vector2(radius_xy.x, radius_xy.y);
        skillRadiusArea.transform.localScale = radius_area;
        skillRadiusArea.SetActive(true);
        Debug.Log(current_casting_skill_name);
        isActivingSkill = true;
        if (current_casting_skill_type == 0) // circle
        {            
            skillRangeAreaCircle.transform.localScale = range_area;            
            skillRangeAreaCircle.SetActive(true);
        }
        else if (current_casting_skill_type == 1) // bar
        {
            skillRangeAreaBar.transform.localScale = range_area;
            oriSkillRangeAreaBar = range_area;
            skillRangeAreaBar.SetActive(true);
        }
        else if (current_casting_skill_type == 2 || current_casting_skill_type == 3 || current_casting_skill_type == 4) // target
        {            
            skillRangeAreaTargeting.SetActive(true);
        }
        else if (current_casting_skill_type == 5 || current_casting_skill_type == 6 || current_casting_skill_type == 7) // buff
        {
            if (current_casting_skill_type == 5) // self
            {
                CastingSkill(transform.position, gameObject);
            }
            else if(current_casting_skill_type == 6) // player
            {
                CastingSkill(transform.position, playerGroup);
            }
            else if(current_casting_skill_type == 7) // enemy
            {
                CastingSkill(transform.position, enemyGroup);
            }
        }
            
    }

    void CastingSkill(Vector2 skillPos, GameObject target = null)
    {        
        castSkill = CastSkill(skillPos, target);
        StartCoroutine(castSkill);
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
        if (current_casting_skill_type == 0 ||current_casting_skill_type == 2 || current_casting_skill_type == 3 || current_casting_skill_type == 4) // circle or target
        {
            //first, if target position is outside the skill range, move to skill range 
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
        goalPos = transform.position;
        movable = false;
        characterAnimator.SetBool("IsRunning", false);        
        characterAnimator.SetTrigger(SkillManager.instance.skillData[current_casting_skill_name].animType);

        if (current_casting_skill_type == 0) // circle
        {
            object[] Params = new object[2];
            Params[0] = skillPos;
            Params[1] = SkillManager.instance.skillData[current_casting_skill_name].skillDuration;
            SkillManager.instance.SendMessage(current_casting_skill_name, Params);
        }        
        else if(current_casting_skill_type == 1) // bar
        {            
            Vector2 _dirMVec = (skillPos - (Vector2)transform.position).normalized;
            if (_dirMVec.x > 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (_dirMVec.x < 0) transform.localScale = new Vector3(1, 1, 1);
            skillPos += new Vector2 (0f, 0.3f);
            object[] Params = new object[3];
            Params[0] = skillPos;
            Params[1] = skillCastingPosition.position;
            Params[2] = gameObject;
            SkillManager.instance.SendMessage(current_casting_skill_name, Params);
            if(current_casting_skill_name == "arrow_dash")
                goalPos = skillPos;
        }
        else if (current_casting_skill_type == 2 || current_casting_skill_type == 3 || current_casting_skill_type == 4) // 2:target only character,  3:only monster  4:both
        {
            object[] Params = new object[3];
            Params[0] = gameObject;
            Params[1] = targetObject;
            Params[2] = SkillManager.instance.skillData[current_casting_skill_name].skillDuration;
            SkillManager.instance.SendMessage(current_casting_skill_name, Params);
        }
        else if(current_casting_skill_type == 5 || current_casting_skill_type == 6 || current_casting_skill_type == 7) // buff
        {            
            object[] Params = new object[1];
            Params[0] = targetObject;
            SkillManager.instance.SendMessage(current_casting_skill_name, Params);
        }

        float delay = 0;
        while (delay < SkillManager.instance.skillData[current_casting_skill_name].skillDelay)
        {
            delay += Time.deltaTime;
            yield return null;
        }
        movable = true;
    }



    void getItem(GameObject got_item)
    {
        string got_item_name = got_item.transform.GetChild(0).name.Split(" ")[0];
        int got_item_cnt = int.Parse(got_item.transform.GetChild(0).name.Split(" ")[1]);
        Sprite got_item_sprite = got_item.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
        Debug.Log(got_item_name);
        Debug.Log(itemsInInventory);
        if (itemsInInventory.ContainsKey(got_item_name))
        {
            int item_index = itemsInInventory[got_item_name];
            int inventory_item_cnt = int.Parse(itemBox.transform.GetChild(item_index).GetChild(1).GetComponent<TMP_Text>().text);
            inventory_item_cnt += got_item_cnt;
            itemBox.transform.GetChild(item_index).GetChild(1).GetComponent<TMP_Text>().text = inventory_item_cnt.ToString();
            Destroy(got_item);
        }
        else
        {
            int inventory_cnt = itemBox.transform.childCount;
            if (inventory_cnt < maxInventoryCnt)
            {
                itemsInInventory.Add(got_item_name, inventory_cnt);
                GameObject new_item = Instantiate(gettingItem);
                new_item.transform.GetChild(0).GetComponent<Image>().sprite = got_item_sprite;
                new_item.transform.GetChild(1).GetComponent<TMP_Text>().text = got_item_cnt.ToString();
                new_item.transform.SetParent(itemBox.transform);
                new_item.SetActive(true);
                Destroy(got_item);
            }
        }
    }
    void Move_Character()
    {
        Vector3 _dirVec = goalPos - transform.position;        
        if (_dirVec.sqrMagnitude < 0.001f)
        {
            characterAnimator.SetBool("IsRunning", false);
            return;
        }
        Vector3 _dirMVec = _dirVec.normalized;
        if (_dirMVec.x > 0) transform.localScale = new Vector3(-1, 1, 1);
        else if (_dirMVec.x < 0) transform.localScale = new Vector3(1, 1, 1);
        transform.position += (_dirMVec * characterMoveSpeed * Time.deltaTime);
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
            now_scale_x += pointSpeed*Time.deltaTime;
            now_scale_y += pointSpeed*Time.deltaTime;
            new_move_pointer.transform.localScale = new Vector2(now_scale_x, now_scale_y);
            yield return null;
        }
        while (new_move_pointer.transform.localScale.x >= 0)
        {
            float now_scale_x = new_move_pointer.transform.localScale.x;
            float now_scale_y = new_move_pointer.transform.localScale.x;
            now_scale_x -= pointSpeed*Time.deltaTime;
            now_scale_y -= pointSpeed*Time.deltaTime;
            new_move_pointer.transform.localScale = new Vector2(now_scale_x, now_scale_y);
            yield return null;
        }
        Destroy(new_move_pointer);
    }


}

