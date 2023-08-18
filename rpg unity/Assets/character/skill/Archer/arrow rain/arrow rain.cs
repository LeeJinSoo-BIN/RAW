using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrowdrop : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject rain;
    bool activate;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activate)
        {
            activate = false;

        }
    }

    IEnumerator drop()
    {

        yield return null;
    }
}
