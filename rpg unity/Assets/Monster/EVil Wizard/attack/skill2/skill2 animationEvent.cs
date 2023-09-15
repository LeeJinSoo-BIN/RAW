using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class skill2animationEvent : MonoBehaviour
{

    public BoxCollider2D handBox;
    public BoxCollider2D bombBox;
    private void Start()
    {
        //animator = GetComponent<Animator>();
        handBox.enabled = false;
        bombBox.enabled = false;
    }

    public void Handing()
    {        
        transform.parent.GetComponent<EvilWizardSkill2>().hand = true;
    }
    public void explose()
    {        
        transform.parent.GetComponent<EvilWizardSkill2>().bomb = true;
        bombBox.enabled = true;
    }
}
