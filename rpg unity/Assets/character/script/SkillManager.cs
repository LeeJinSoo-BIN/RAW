using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SkillManager instance;

    public GameObject skill_magic_floor;
    public GameObject skill_magic_totem;
    
    public struct skill_spec
    {
        public int castType; 
        // 0: circle   1: bar   2: targeting    3: buff
        // 4: charging


        public (float x, float y) radius;
        public (float x, float y) range;

        public string animType; //attack1,2,3, skill1,2,3        
        public float skillDelay;
        public float skillDuration;
        /*public skill_spec(int _castType, (float x, float y) _radius, (float x, float y) _range, int _animType, int _effectType)
        {
            castType = _castType;
            radius = _radius;
            range = _range;
            animType = _animType;
            effectType = _effectType;
        }*/

        
    };
    public Dictionary<string, skill_spec> skillData = new Dictionary<string, skill_spec>();    
    public Dictionary<string, Dictionary<string, string>> rollSkills = new Dictionary<string, Dictionary<string, string>>();
    private void Awake()
    {
        if (SkillManager.instance == null)
            SkillManager.instance = this;

        skill_spec tmp_skill;
        tmp_skill.castType = 0;
        tmp_skill.radius = (1f, 1f);
        tmp_skill.range = (1f, 1f);
        tmp_skill.animType = "attack3";        
        tmp_skill.skillDelay = 1f;
        tmp_skill.skillDuration = 3f;
        skillData.Add("magic_floor", tmp_skill);

        tmp_skill.castType = 0;
        tmp_skill.radius = (1.5f, 1.5f);
        tmp_skill.range = (2f, 2f);
        tmp_skill.animType = "attack3";
        tmp_skill.skillDelay = 1f;
        tmp_skill.skillDuration = 10f;
        skillData.Add("magic_totem", tmp_skill);
        //skillData.Add("magic_heal", new skill_spec(2, (1.2f, 1.2f), (1.2f, 1.2f), 2, 1));
        //skillData.Add("magic_totem", new skill_spec(0, (2f, 2f), (0.4f, 0.4f), 4, 0));
        //skillData.Add("magic_global_heal", new skill_spec(3, (2.2f, 2.2f), (1.1f, 1.1f), 5, 1));


        //0 - arrow
        Dictionary<string, string> rollSkill = new Dictionary<string, string>();
        rollSkill.Add("Q", "arrow_rain");
        rollSkill.Add("W", "arrow_dash");
        rollSkill.Add("E", "arrow_gatling");
        rollSkill.Add("R", "arrow_charge");
        rollSkills.Add("arrow", rollSkill);
        //1 - sword      
        rollSkill = new Dictionary<string, string>();
        rollSkill.Add("Q", "sword_smash");
        rollSkill.Add("W", "sword_shield");
        rollSkill.Add("E", "sword_slash");
        rollSkill.Add("R", "sword_bind");
        rollSkills.Add("sword", rollSkill);

        //2 - magic
        rollSkill = new Dictionary<string, string>();
        rollSkill.Add("Q", "magic_floor");
        rollSkill.Add("W", "magic_heal");
        rollSkill.Add("E", "magic_totem");
        rollSkill.Add("R", "magic_global_heal");
        rollSkills.Add("magic", rollSkill);        


    }
    void magic_floor(object[] _params)
    {   
        Vector2 pos = (Vector2)_params[0];
        float duration = (float)_params[1];
        GameObject magicfloor = Instantiate(skill_magic_floor);
        magicfloor.transform.position = pos;
        StartCoroutine(Vanish(duration, magicfloor));
    }

    void magic_totem(object[] _params)
    {
        Vector2 pos = (Vector2)_params[0];
        float duration = (float)_params[1];
        GameObject magicTotem = Instantiate(skill_magic_totem);
        magicTotem.transform.position = pos;
        StartCoroutine(Vanish(duration, magicTotem));
    }
    IEnumerator Vanish(float duration, GameObject who)
    {        
        float time = 0;
        while (time < duration)
        {            
            time += Time.deltaTime;
            yield return null;
        }        
        who.GetComponent<Animator>().SetTrigger("vanish");
        time = 0;
        while (time < 0.45)
        {
            time += Time.deltaTime;
            yield return null;
        }        
        Destroy(who);
    }
    
 
}
