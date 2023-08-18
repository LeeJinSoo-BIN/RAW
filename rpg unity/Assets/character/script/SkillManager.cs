using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SkillManager instance;

    public GameObject skill_magic_floor;
    public GameObject skill_magic_totem;
    public GameObject skill_magic_heal;
    //public GameObject skill_magic_gobal_heal;
    public GameObject skill_arrow_rain;
    //public GameObject skill_arrow_dash;
    public GameObject skill_arrow_gatling;
    public GameObject skill_arrow_charge;
    public GameObject skill_sword_shield;


    public Transform skillCastPos;
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

        tmp_skill.castType = 2;
        tmp_skill.radius = (1.5f, 1.5f);
        tmp_skill.range = (2f, 2f);
        tmp_skill.animType = "attack3";
        tmp_skill.skillDelay = 1f;
        tmp_skill.skillDuration = 3f;
        skillData.Add("magic_heal", tmp_skill);


        tmp_skill.castType = 6;
        tmp_skill.radius = (0f, 0f);
        tmp_skill.range = (0f, 0f);
        tmp_skill.animType = "attack3";
        tmp_skill.skillDelay = 1f;
        tmp_skill.skillDuration = 3f;
        skillData.Add("magic_global_heal", tmp_skill);



        tmp_skill.castType = 0;
        tmp_skill.radius = (2f, 2f);
        tmp_skill.range = (1f, 1f);
        tmp_skill.animType = "skillBow1";
        tmp_skill.skillDelay = 1f;
        tmp_skill.skillDuration = 1.5f;
        skillData.Add("arrow_rain", tmp_skill);

        tmp_skill.castType = 1;
        tmp_skill.radius = (0.8f, 0.8f);
        tmp_skill.range = (0.8f, 0.7f);
        tmp_skill.animType = "dash";
        tmp_skill.skillDelay = 0.5f;
        tmp_skill.skillDuration = 0f;
        skillData.Add("arrow_dash", tmp_skill);

        tmp_skill.castType = 1;
        tmp_skill.radius = (2f, 2f);
        tmp_skill.range = (2f, 1f);
        tmp_skill.animType = "skillBow1";
        tmp_skill.skillDelay = 1f;
        tmp_skill.skillDuration = 0f;
        skillData.Add("arrow_gatling", tmp_skill);

        tmp_skill.castType = 1;
        tmp_skill.radius = (20f, 20f);
        tmp_skill.range = (20f, 8f);
        tmp_skill.animType = "skillBow2";
        tmp_skill.skillDelay = 3f;
        tmp_skill.skillDuration = 0f;
        skillData.Add("arrow_charge", tmp_skill);


        tmp_skill.castType = 5;
        tmp_skill.radius = (0f, 0f);
        tmp_skill.range = (0f, 0f);
        tmp_skill.animType = "skillSword1";
        tmp_skill.skillDelay = 0.5f;
        tmp_skill.skillDuration = 0f;
        skillData.Add("sword_shield", tmp_skill);
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


    void magic_heal(object[] _params)
    {
        GameObject target = (GameObject)_params[0];
        GameObject magicHeal = Instantiate(skill_magic_heal, target.transform);
        magicHeal.transform.localPosition = Vector2.zero;
        StartCoroutine(Vanish(0.3f, magicHeal));
    }

    void magic_global_heal(object[] _params)
    {
        Debug.Log("magic_global_heal");
        GameObject target = (GameObject)_params[0];
        for(int k = 0; k < target.transform.childCount; k++)
        {
            GameObject magicHeal = Instantiate(skill_magic_heal, target.transform.GetChild(k).transform);
            magicHeal.transform.localPosition = Vector2.zero;
            StartCoroutine(Vanish(0.3f, magicHeal));
        }
    }

    void arrow_rain(object[] _params)
    {
        Vector2 pos = (Vector2)_params[0];
        float duration = (float)_params[1];
        GameObject arrowRain = Instantiate(skill_arrow_rain);
        arrowRain.transform.position = pos;
        StartCoroutine(Vanish(duration, arrowRain));
    }

    void arrow_dash(object[] _params)
    {
        Vector2 desPos = (Vector2)_params[0];
        Vector3 oriPos = (Vector3)_params[1];
        GameObject character = (GameObject)_params[2];
        StartCoroutine(Dash(desPos, character, 5f));
    }

    void arrow_gatling(object[] _params)
    {
        Vector2 desPos = (Vector2)_params[0];
        Vector3 oriPos = (Vector3)_params[1];
        //GameObject character = (GameObject)_params[2];

        StartCoroutine(Gatling(oriPos, desPos));
    }
    void arrow_charge(object[] _params)
    {
        Vector2 desPos = (Vector2)_params[0];
        Vector3 oriPos = (Vector3)_params[1];
        GameObject new_arrow = Instantiate(skill_arrow_charge);
        desPos *= 5;
        new_arrow.transform.position = oriPos;
        new_arrow.GetComponent<arrowcharge>().targetPos = desPos;
    }


    void sword_shield(object[] _params)
    {
        GameObject target = (GameObject)_params[0];
        GameObject swordShield = Instantiate(skill_sword_shield, target.transform);
        swordShield.transform.localPosition = Vector2.zero;
        StartCoroutine(Vanish(0.7f, swordShield));
    }


    IEnumerator Gatling(Vector2 oriPos, Vector2 desPos)
    {

        for(int k = 0; k < 20; k++)
        {
            float _time = 0;
            while(true)
            {
                if(_time > 0.05f)
                {
                    break;
                }
                _time += Time.deltaTime;
                yield return null;
            }
            float rand_x = Random.Range(-0.1f, 0.1f);
            float rand_y = Random.Range(-0.1f, 0.1f);
            GameObject new_arrow = Instantiate(skill_arrow_gatling);
            new_arrow.transform.position = oriPos + new Vector2(rand_x, rand_y);
            new_arrow.GetComponent<arrowgatling>().targetPos = desPos + new Vector2(rand_x, rand_y);
        }
    }

    IEnumerator Dash(Vector3 goalPos, GameObject who, float speed)
    {
        while (true)
        {
            Vector2 _dirVec = goalPos - who.transform.position;
            Vector2 _disVec = (Vector2)goalPos - (Vector2)who.transform.position;
            Vector3 _dirMVec = _dirVec.normalized;
            if (_dirVec.sqrMagnitude < 0.001f)
            {
                who.transform.position = goalPos;
                break;
            }
            if (_dirMVec.x > 0) who.transform.localScale = new Vector3(-1, 1, 1);
            else if (_dirMVec.x < 0) who.transform.localScale = new Vector3(1, 1, 1);

            who.transform.position += (_dirMVec * speed * Time.deltaTime);
            yield return null;
        }


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
