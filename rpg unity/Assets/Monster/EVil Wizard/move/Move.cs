using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject wizard;

    public SpriteRenderer ewrend;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.RightArrow))
        {
            ewrend.flipX = false;
            anim.SetBool("moving", true);
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            ewrend.flipX = true;
            anim.SetBool("moving", true);
        }
        else anim.SetBool("moving", false);
        
    }
}
