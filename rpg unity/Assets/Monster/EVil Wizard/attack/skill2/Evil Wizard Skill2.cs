using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using Photon.Pun;
using UnityEngine.TextCore.Text;

public class EvilWizardSkill2 : MonoBehaviour
{
    //public GameObject character; // 캐릭터의 위치
    //private Animator animator; // 애니메이터 컴포넌트

    private float handDeal;
    private float bombDeal;   
    public bool hand = false;
    public bool bomb = false;
    
    public void ExecuteSkill()
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        // 캐릭터의 위치로 불 생성

        while (true)
        {
            if(bomb)
            {
                float time = 0f;
                while(true)
                {
                    time += Time.deltaTime;
                    if(time > 0.5f)
                    {
                        Destroy(gameObject);
                        break;
                    }
                    yield return null;
                }
            }
            yield return null;
        }

        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {        
        if(collision == null)
            return;
        if (collision.CompareTag("Player") && collision.name == "foot")
        {
            if (collision.transform.parent.GetComponent<PhotonView>().IsMine )
            {
                if (hand)
                {
                    CharacterState state = collision.transform.parent.GetComponentInChildren<CharacterState>();
                    state.ProcessSkill(0, handDeal);
                }
                if (bomb)
                {
                    CharacterState state = collision.transform.parent.GetComponentInChildren<CharacterState>();
                    state.ProcessSkill(0, bombDeal);
                }
            }
        }
    }

    [PunRPC]
    void InitSkill(Vector3 position, string targetName, float[] deal)
    {
        //startPosition = position;
        //character = GameObject.Find(targetName);
        handDeal = deal[0];
        bombDeal = deal[1];
        ExecuteSkill();
    }
}

