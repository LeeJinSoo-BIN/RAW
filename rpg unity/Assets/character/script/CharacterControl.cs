using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;


public class CharacterControl : MonoBehaviour
{
    // Start is called before the first frame update   

    public bool movable = true;
    public GameObject movePointer;
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
    public GameObject skillRangeAreaCircle;
    public GameObject skillRangeAreaBar;
    private bool isActivingSkill = false;
    private string current_casting_skill;

    
    

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
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.name);
                    if (hit.collider.CompareTag("Ground") || (hit.collider.CompareTag("SkillArea")))
                    {
                        goalPos = hit.point;
                        StartCoroutine(pointingGoal(goalPos));
                        if (isActivingSkill)
                        {
                            isActivingSkill = false;                            
                        }
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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Item"))
                {
                    if(hit.transform.GetChild(1).gameObject.activeSelf)
                        getItem(hit.transform.gameObject);
                }
                if (hit.collider.CompareTag("SkillArea"))
                {
                    if (isActivingSkill)
                    {
                        CastingSkill();
                    }
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

                string now_input = Input.inputString.ToUpper();
                if (now_input == current_casting_skill)
                    deactivateSkill();
                else
                {
                    deactivateSkill();
                    activeSkill(now_input);
                }
                
            }
        }
        if (isActivingSkill)
        { 
            Vector3 mousePos = Input.mousePosition;                        
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            mousePos = new Vector3(mousePos.x, mousePos.y, -1);

            (float x, float y)radius_xy = SkillManager.instance.skillData[current_casting_skill].radius;
            Vector2 radius_area = new Vector2(radius_xy.x, radius_xy.y);
            skillRadiusArea.transform.localScale = radius_area;
            skillRadiusArea.SetActive(true);


            if (SkillManager.instance.skillData[current_casting_skill].type == 0)
            {
                skillRangeAreaCircle.transform.position = mousePos;
            }
            else if(SkillManager.instance.skillData[current_casting_skill].type == 1)
            {
                Vector2 target = skillRangeAreaBar.transform.position;
                float angle = Mathf.Atan2(mousePos.y - target.y, mousePos.x - target.x) * Mathf.Rad2Deg;
                Debug.Log(angle);
                skillRangeAreaBar.transform.rotation = Quaternion.AngleAxis(angle - 180, Vector3.forward);
            }


            

        }
                
        
    }
    void deactivateSkill()
    {
        skillRadiusArea.SetActive(false);
        skillRangeAreaCircle.SetActive(false);
        skillRangeAreaBar.SetActive(false);
        current_casting_skill = "";
        isActivingSkill = false;
    }
    void activeSkill(string now_skill)
    {
        current_casting_skill = now_skill;
        (float x, float y) range_xy = SkillManager.instance.skillData[current_casting_skill].range;
        Vector2 range_area = new Vector2(range_xy.x, range_xy.y);
        if (SkillManager.instance.skillData[current_casting_skill].type == 0)
        {            
            skillRangeAreaCircle.transform.localScale = range_area;            
            skillRangeAreaCircle.SetActive(true);
        }
        else if (SkillManager.instance.skillData[current_casting_skill].type == 1)
        {
            skillRangeAreaBar.transform.localScale = range_area;
            skillRangeAreaBar.SetActive(true);
        }
        isActivingSkill = true;
    }

    void CastingSkill()
    {

        isActivingSkill = false;
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
        Vector3 _disVec = (Vector2)goalPos - (Vector2)transform.position;
        if (_disVec.sqrMagnitude < 0.001f)
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

