using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSmash : MonoBehaviour
{
    private int flatDeal = 1;
    private int dealIncreasePerSkillLevel = 1;
    private int dealIncreasePerPower = 1;
    public GameObject target;
    

    public void Deal()
    {
        CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(0, "sword_smash", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
    }
}
