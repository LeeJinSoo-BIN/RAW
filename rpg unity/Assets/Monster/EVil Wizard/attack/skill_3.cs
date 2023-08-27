using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class skill_3 : MonoBehaviour
{
    
    private float Damage = 60f;
    // Start is called before the first frame update
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {            
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(0, Damage);
        }
    }
}
