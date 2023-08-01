using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SkillManager instance;


    public struct skill_spec
    {
        public int type; // 0 : circle   1 : bar
        public (float x, float y) radius;

        public (float x, float y) range;
        public skill_spec(int _type, (float x, float y) _radius, (float x, float y) _range)
        {
            type = _type;
            radius = _radius;
            range = _range;
        }
    };
    public Dictionary<string, skill_spec> skillData = new Dictionary<string, skill_spec>();

    private void Awake()
    {
        if (SkillManager.instance == null)
            SkillManager.instance = this;
        skillData.Add("Q", new skill_spec(0, (1f, 1f), (0.5f, 0.5f)));
        skillData.Add("W", new skill_spec(1, (1.2f, 1.2f), (1.2f, 1.2f)));
        skillData.Add("E", new skill_spec(0, (2f, 2f), (0.4f, 0.4f)));
        skillData.Add("R", new skill_spec(0, (2.2f, 2.2f), (1.1f, 1.1f)));

    }
    void Start()
    {


    }
}
