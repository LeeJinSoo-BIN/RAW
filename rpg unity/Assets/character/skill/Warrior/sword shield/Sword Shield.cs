using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordShield : MonoBehaviour
{
    private int flatShield = 1;
    private int shieldIncreasePerSkillLevel = 1;
    private int shieldIncreasePerPower = 1;
    public GameObject target;

    public void Shield()
    {
        CharacterState state = target.transform.GetComponentInChildren<CharacterState>();
        state.ProcessSkill(2, "sword_shield", flatShield, shieldIncreasePerSkillLevel, shieldIncreasePerPower);
    }
}
