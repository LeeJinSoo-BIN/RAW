using UnityEditor;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;


public class skill_1 : MonoBehaviourPunCallbacks
{

    public Vector3 startPosition;
    public GameObject character;
    private Animator animator;     // ?????????? ????????
    private float speed = 3f;
    private bool bomb = false;
    private CircleCollider2D skillCollider;

    private float flatDeal = 50f;
    private float level = 1;
    private void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        skillCollider = gameObject.GetComponent<CircleCollider2D>();
        skillCollider.enabled = false;
        StartCoroutine(Vanish(15f));
    }


    IEnumerator Vanish(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }        
        Destroy(gameObject, 0.45f);
    }

    public void ExecuteSkill()
    {
        // ?? ?????????? ???? ?????? ????
        StartCoroutine(MoveFireRoutine());        
    }

    private IEnumerator MoveFireRoutine()
    {        
        Vector3 targetPosition = new Vector3(character.transform.position.x, character.transform.position.y + 0.7f); ;/* ???????? ???? Transform */
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;


        while (true)
        {
            float distanceCovered = (Time.time - startTime) * speed; // ???? ???? ???? ????
            float fractionOfJourney = distanceCovered / journeyLength;
            gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            if ((gameObject.transform.position - targetPosition).magnitude <= 0.1f)
            {
                // ???? ?????? ?????? ???????? bomb ?????? ??????
                animator.SetTrigger("bomb");
                startTime = Time.time;
                bomb = true;
                break;
            }
            yield return null;
        }
        while (true)
        {
            if (Time.time - startTime > 0.3f && bomb)
            {
                skillCollider.enabled = true;
                bomb = false;
            }
            if (Time.time - startTime >= 1.5f)
            {
                Destroy(gameObject);
                break;
            }
            yield return null;
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.transform.GetComponent<PhotonView>().IsMine)
            {
                CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
                state.ProcessSkill(0, flatDeal * level);
            }
        }
    }

    [PunRPC]
    void InitSkill(Vector3 position, string targetName)
    {
        startPosition = position;
        character = GameObject.Find(targetName);
        ExecuteSkill();
    }
}
