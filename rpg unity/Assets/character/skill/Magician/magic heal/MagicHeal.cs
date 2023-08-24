using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicHeal : MonoBehaviour
{
    // Start is called before the first frame update

    private int flatHeal = 1;
    private int healIncreasePerSkillLevel = 1;
    private int healIncreasePerPower = 1;

    void Start()
    {
        CharacterState state = transform.parent.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(1, "magic_heal", flatHeal, healIncreasePerSkillLevel, healIncreasePerPower);
    }

    
}
