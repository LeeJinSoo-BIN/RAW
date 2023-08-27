using UnityEditor;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class skill_1 : MonoBehaviourPunCallbacks
{

    public Vector3 startPosition;
    public GameObject character;
    private Animator animator;     // ?????????? ????????
    private float speed = 3f;
    private bool bomb = false;
    private CircleCollider2D collider;

    private float flatDeal = 50f;
    private float level = 1;
    private void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        collider = gameObject.GetComponent<CircleCollider2D>();
        collider.enabled = false;
    }

   
    public void ExecuteSkill()
    {
        // ?? ?????????? ???? ?????? ????
        StartCoroutine(MoveFireRoutine());        
    }

    private System.Collections.IEnumerator MoveFireRoutine()
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
                collider.enabled = true;
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
            CharacterState state = collision.transform.GetComponentInChildren<CharacterState>();
            state.ProcessSkill(0, flatDeal * level);
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
