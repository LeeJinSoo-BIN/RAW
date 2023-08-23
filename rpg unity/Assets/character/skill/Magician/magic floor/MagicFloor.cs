using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicFloor : MonoBehaviour
{
    // Start is called before the first frame update
    private float healCycle = 0.3f;
    private float time = 0;
    private bool healable = false;
    public int healAmount;

    // Update is called once per frame
    void Update()
    {
        if (time > healCycle) {
            healable = true;
            time = 0;
        }
        else
        {
            healable = false;
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

        }
        else if (collision.CompareTag("Player"))
        {
            if (healable)
            {
                Debug.Log("heal");
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("heal");
    }
    
}
