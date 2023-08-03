using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fadeout : StateMachineBehaviour
{
    bool isFadeOutComplete = false;

    // 애니메이션 상태가 시작될 때 호출됨
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 시작할 때 자식 오브젝트를 비활성화
        animator.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        isFadeOutComplete = false;
    }

    // 애니메이션 상태가 업데이트될 때 호출됨
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
                float alpha = 1f - (timePassed / fadeoutDuration); // 페이드 아웃 효과를 위한 alpha 계산
                animator.gameObject.GetComponent<SpriteRenderer>().color = new Color(objectColor.r, objectColor.g, objectColor.b, alpha);
                
            }
        }
    }
}
