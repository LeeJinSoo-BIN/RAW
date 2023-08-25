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
        Debug.Log("hand!");
        transform.parent.GetComponent<skill_2>().hand = true;        
        handBox.enabled = true;
    }
    public void explose()
    {
        Debug.Log("bomb!");
        transform.parent.GetComponent<skill_2>().bomb = true;
        bombBox.enabled = true;
    }
}
