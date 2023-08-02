using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkill : MonoBehaviour
{

    public bool attackable = true;
    public GameObject skillRadiusArea;
    public GameObject skillRangeArea;
    private bool isActivingSkill = false;
    private string current_casting_skill;
    public Animator characterAnimator;
    private Vector3 goalPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
                    isActivingSkill = false;
                else
                    isActivingSkill = true;
                current_casting_skill = now_input;

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
                (float x, float y) range_xy = SkillManager.instance.skillData[current_casting_skill].range;
                Vector2 range_area = new Vector2(range_xy.x, range_xy.y);
                skillRangeArea.transform.localScale = range_area;
                skillRangeArea.transform.position = mousePos;
                skillRangeArea.SetActive(true);
            }
            else if (SkillManager.instance.skillData[current_casting_skill].castType == 1)
            {

            }




        }
        else
        {
            skillRadiusArea.SetActive(false);
            skillRangeArea.SetActive(false);
            current_casting_skill = "";
        }
    }
}
