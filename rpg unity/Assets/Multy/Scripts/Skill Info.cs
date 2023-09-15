using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SkillInfo : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
   

    
    public float power;
    public float criticalDamage;
    public float criticalPercent;

    public float flatDeal;
    public float dealIncreasePerSkillLevel;
    public float dealIncreasePerPower;

    public float flatHeal;
    public float healIncreasePerSkillLevel;
    public float healIncreasePerPower;

    public float flatShield;
    public float shieldIncreasePerSkillLevel;
    public float shieldIncreasePerPower;

    public float duration;
    public float skillLevel;

    /*[PunRPC]
    public SkillInfo(SkillSpec currentSkillSpec, CharacterSpec castingCharacterSpec)
    {
        power = castingCharacterSpec.power;
        criticalDamage = castingCharacterSpec.criticalDamage;
        criticalPercent = castingCharacterSpec.criticalPercent;
        flatDeal = currentSkillSpec.flatDeal;
        dealIncreasePerPower = currentSkillSpec.dealIncreasePerPower;
        dealIncreasePerSkillLevel = currentSkillSpec.dealIncreasePerSkillLevel;

        flatHeal = currentSkillSpec.flatHeal;
        healIncreasePerPower = currentSkillSpec.healIncreasePerPower;
        healIncreasePerSkillLevel = currentSkillSpec.healIncreasePerSkillLevel;

        flatShield = currentSkillSpec.flatShield;
        shieldIncreasePerPower = currentSkillSpec.shieldIncreasePerPower;
        shieldIncreasePerSkillLevel = currentSkillSpec.shieldIncreasePerSkillLevel;

        duration = currentSkillSpec.duration;
        //skillLevel = castingCharacterSpec.skillLevel[currentSkillSpec.skillName];
    }    */
    
    /*public float dealWithoutCri()
    {        
        return SkillManager.instance.CaculateCharacterSkillDamage(skillLevel, power,
           flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
    }
    
    public float deal()
    {
        return SkillManager.instance.CaculateCharacterSkillDamage(skillLevel, power,
            flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower, criticalPercent, criticalDamage, true);
    }

    public float heal()
    {
        return SkillManager.instance.CaculateCharacterSkillDamage(skillLevel, power,
            flatHeal, healIncreasePerSkillLevel, healIncreasePerPower);
    }
    public float shield()
    {
        return SkillManager.instance.CaculateCharacterSkillDamage(skillLevel, power,
            flatShield, shieldIncreasePerSkillLevel, shieldIncreasePerPower);
    }*/

}
