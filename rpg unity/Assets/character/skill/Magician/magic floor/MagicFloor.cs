using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicFloor : MonoBehaviour
{
    // Start is called before the first frame update
    private float cycle = 0.3f;
    private float time = 0;
    private bool active = false;
    

    private int flatDeal = 1;
    private int dealIncreasePerSkillLevel = 1;
    private int dealIncreasePerPower = 1;

    private int flatHeal = 1;    
    private int healIncreasePerSkillLevel = 1;    
    private int healIncreasePerPower = 1;    

    
    // Update is called once per frame
    void Update()
    {
        if (time > cycle) {
            active = true;
            time = 0;
        }
        else
        {
            active = false;
            time += Time.deltaTime;
        }
    }

    public void selfDestroy()
    {
        Destroy(this);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {        
        if (collision.CompareTag("Monster"))
        {
            if (active)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, "magic_floor", flatDeal, dealIncreasePerSkillLevel, dealIncreasePerPower);
            }
        }
        else if (collision.CompareTag("Player"))
        {
            if (active)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(1, "magic_floor", flatHeal, healIncreasePerSkillLevel, healIncreasePerPower);                
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("heal");
    }
    
}
