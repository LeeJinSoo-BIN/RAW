using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackbutton : MonoBehaviour
{
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim.SetBool("attackbutton", false);
    }

    // Update is called once per frame
    public void Attackbutton()
    {
        anim.SetBool("attackbutton", true);
        StartCoroutine(ResetAttackButton());
    }

    IEnumerator ResetAttackButton()
    {
        // Wait for a short period of time (e.g., 1 second) before resetting the attackbutton parameter to false.
        yield return new WaitForSeconds(0.8f);

        // Reset the attackbutton parameter to false to transition back to idle state.
        anim.SetBool("attackbutton", false);
    }
}
