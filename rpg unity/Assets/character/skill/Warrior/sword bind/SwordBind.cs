using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBind : MonoBehaviour
{
    private float flatDeal = 1;
    private float dealIncreasePerSkillLevel = 1;
    private float dealIncreasePerPower = 1;
    public GameObject target;
    public float duration;
    public void Bind()
    {
        CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(0, "sword_bind", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower, duration:duration);
    }
}
