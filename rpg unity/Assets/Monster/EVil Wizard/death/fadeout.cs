using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fadeout : StateMachineBehaviour
{
    bool isFadeOutComplete = false;

    // �ִϸ��̼� ���°� ���۵� �� ȣ���
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ������ �� �ڽ� ������Ʈ�� ��Ȱ��ȭ
        animator.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        isFadeOutComplete = false;
    }

    // �ִϸ��̼� ���°� ������Ʈ�� �� ȣ���
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!isFadeOutComplete)
        {
            float fadeoutDuration = 1f; // Fadeout duration in seconds
            float timePassed = stateInfo.normalizedTime * stateInfo.length;
            if (timePassed >= fadeoutDuration)
            {
                isFadeOutComplete = true;
              
                animator.gameObject.transform.GetChild(0).gameObject.SetActive(true);

                
            }
            else
            {
                Color objectColor = animator.gameObject.GetComponent<SpriteRenderer>().color;
                float alpha = 1f - (timePassed / fadeoutDuration); // ���̵� �ƿ� ȿ���� ���� alpha ���
                animator.gameObject.GetComponent<SpriteRenderer>().color = new Color(objectColor.r, objectColor.g, objectColor.b, alpha);
                
            }
        }
    }
}
