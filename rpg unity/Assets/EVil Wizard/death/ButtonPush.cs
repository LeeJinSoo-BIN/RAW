using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPush : MonoBehaviour
{
    public Animator anim;
    
    // Start is called before the first frame update
    void Start()
    {
        anim.SetBool("ButtonPush", false);

    }

    // Update is called once per frame
    public void buttonPush()
    {
        anim.SetBool("ButtonPush", true);
        
    }


}
