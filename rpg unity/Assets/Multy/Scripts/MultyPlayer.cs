using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MultyPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool movable = true;
    public GameObject movePointer;
    public LayerMask groundLayer;

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
    private bool isActivingSkill = false;
    private string current_casting_skill;
    private Vector2 oriSkillRangeAreaBar;
    private IEnumerator castSkill;

    // Multy
    public Rigidbody2D RB;
    public PhotonView PV;
    public Text NickNameText;

    Vector3 curPos;

    private void Awake()
    {
        itemBox = inventoryUi.transform.GetChild(0).GetChild(2).gameObject;
        gettingItem = inventoryUi.transform.GetChild(1).gameObject;
        deactivateSkill();

        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }

    void Update()
    {
        if (PV.IsMine)
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
            if (Input.GetKeyDown(KeyCode.S))
            {
                goalPos = transform.position;
                StopCoroutine(castSkill);
                deactivateSkill();
            }
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
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
                    Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D hit_ground = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);
                    if (hit_ground.collider != null)
                    {
                        CastingSkill(hit_ground.point);
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

                (float x, float y) radius_xy = SkillManager.instance.skillData[current_casting_skill].radius;
                Vector2 radius_area = new Vector2(radius_xy.x, radius_xy.y);
                skillRadiusArea.transform.localScale = radius_area;
                skillRadiusArea.SetActive(true);


                if (SkillManager.instance.skillData[current_casting_skill].castType == 0)
                {
                    skillRangeAreaCircle.transform.position = mousePos;
                }
                else if (SkillManager.instance.skillData[current_casting_skill].castType == 1)
                {
                    //Vector2 target = skillRangeAreaBar.transform.position;
                    Vector2 target = transform.position;
                    float angle_pi = Mathf.Atan2(mousePos.y - target.y, mousePos.x - target.x);
                    float angle_rad = angle_pi * Mathf.Rad2Deg;

                    if (transform.localScale.x > 0)
                        angle_rad -= 180;
                    skillRangeAreaBar.transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);

                    //with cosine function
                    //float ratio = (float)(Mathf.Cos(2 * angle_pi) / 4 + 0.75);

                    /*
                     * 2?? ?????????? ??????
                    angle_pi = Mathf.Abs(angle_pi) / Mathf.PI;
                    float ratio = 2 * angle_pi * angle_pi - 2 * angle_pi + 1;

                    */

                    //??????????
                    float a = 1f; // ?????? ????
                    float b = 0.5f; //?????? ????
                    float slope = (mousePos.y - target.y) / (mousePos.x - target.x);
                    float t = Mathf.Atan((slope * a) / b);
                    float x_intersect = target.x + a * Mathf.Cos(t);
                    float y_intersect = target.y + b * Mathf.Sin(t);
                    float ratio = Mathf.Sqrt((x_intersect - target.x) * (x_intersect - target.x) + (y_intersect - target.y) * (y_intersect - target.y));


                    float scaled_x = oriSkillRangeAreaBar.x * ratio;

                    skillRangeAreaBar.transform.localScale = new Vector2(scaled_x, oriSkillRangeAreaBar.y);
                }
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100)
            transform.position = curPos;
        else
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }
    void deactivateSkill()
    {
        skillRadiusArea.SetActive(false);
        skillRangeAreaCircle.SetActive(false);
        skillRangeAreaBar.SetActive(false);
        current_casting_skill = "";
        isActivingSkill = false;
        //StopCoroutine("CastSkill");
    }
    void activeSkill(string now_skill)
    {
        current_casting_skill = now_skill;
        (float x, float y) range_xy = SkillManager.instance.skillData[current_casting_skill].range;
        Vector2 range_area = new Vector2(range_xy.x, range_xy.y);
        if (SkillManager.instance.skillData[current_casting_skill].castType == 0)
        {            
            skillRangeAreaCircle.transform.localScale = range_area;            
            skillRangeAreaCircle.SetActive(true);
        }
        else if (SkillManager.instance.skillData[current_casting_skill].castType == 1)
        {
            skillRangeAreaBar.transform.localScale = range_area;
            oriSkillRangeAreaBar = range_area;
            skillRangeAreaBar.SetActive(true);
        }
        isActivingSkill = true;
    }

    void CastingSkill(Vector2 skillPos)
    {
        //???? ?????? ???????? ???? ????
        float skill_casting_point_len = Vector2.Distance(skillPos, transform.position);

        //???? ???????? ????
        float skill_radius_len = Vector2.Distance(skillRadiusLengthPoint.transform.position, transform.position);

                
        float angle_pi = Mathf.Atan2(skillPos.y - transform.position.y, skillPos.x - transform.position.x);
        angle_pi = Mathf.Abs(angle_pi) / Mathf.PI;
        float ratio = 2 * angle_pi * angle_pi - 2 * angle_pi + 1;

        skill_radius_len *= ratio;

        //?????? ???? ???? ???? ????


        //?????? ???? ???? ???????? ?????? ????
        castSkill = CastSkill(skillPos);
        StartCoroutine(castSkill);
        deactivateSkill();

    }

    IEnumerator CastSkill(Vector2 skillPos)
    {
        float skill_radius_len = Vector2.Distance(skillRadiusLengthPoint.transform.position, transform.position);

        /*
        float angle_pi = Mathf.Atan2(skillPos.y - transform.position.y, skillPos.x - transform.position.x);
        angle_pi = Mathf.Abs(angle_pi) / Mathf.PI;
        float ratio = 2 * angle_pi * angle_pi - 2 * angle_pi + 1;
        */


        float a = 1f; // ?????? ????
        float b = 0.5f; //?????? ????
        Vector2 target = transform.position;
        float slope = (skillPos.y - target.y) / (skillPos.x - target.x);
        float t = Mathf.Atan((slope * a) / b);
        float x_intersect = target.x + a * Mathf.Cos(t);
        float y_intersect = target.y + b * Mathf.Sin(t);
        float ratio = Mathf.Sqrt((x_intersect - target.x) * (x_intersect - target.x) + (y_intersect - target.y) * (y_intersect - target.y));

        skill_radius_len *= ratio;

        if (SkillManager.instance.skillData[current_casting_skill].castType == 0) // ???????????? ?????????? ???? ?? ??????
        {
            goalPos = skillPos;
            characterAnimator.SetBool("IsRunning", true);
            Move_Character();
            while (true)
            {
                float skill_casting_point_len = Vector2.Distance(skillPos, transform.position);
                if (skill_radius_len >= skill_casting_point_len)
                {
                    goalPos = transform.position;
                    break;
                }
                Debug.Log("moving for skill");
                yield return null;
            }
        }

        // ???? ?????? ??????

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
