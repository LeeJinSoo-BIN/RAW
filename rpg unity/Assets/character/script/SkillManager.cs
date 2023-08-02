using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static SkillManager instance;


    public struct skill_spec
    {
        public int castType; // 0 : circle   1 : bar
        public (float x, float y) radius;
        public (float x, float y) range;

        public int animType; //0 1 2, 3 4 5
        public int effectType; //0 1 2
        public skill_spec(int _castType, (float x, float y) _radius, (float x, float y) _range, int _animType, int _effectType)
        {
            castType = _castType;
            radius = _radius;
            range = _range;
            animType = _animType;
            effectType = _effectType;
        }

        
    };
    public Dictionary<string, skill_spec> skillData = new Dictionary<string, skill_spec>();

    private void Awake()
    {
        if (SkillManager.instance == null)
            SkillManager.instance = this;
        skillData.Add("Q", new skill_spec(0, (1f, 1f), (0.5f, 0.5f), 3, 0));
        skillData.Add("W", new skill_spec(1, (1.2f, 1.2f), (1.2f, 1.2f), 2, 1));
        skillData.Add("E", new skill_spec(0, (2f, 2f), (0.4f, 0.4f), 4, 0));
        skillData.Add("R", new skill_spec(0, (2.2f, 2.2f), (1.1f, 1.1f), 5, 1));

    }
    void Start()
    {


    }
}
