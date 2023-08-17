using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobControl : MonoBehaviour
{
    public GameObject character; // Reference to the character GameObject
    public float moveSpeed = 0.8f; // Speed at which the mob moves
    public float followDistance = 5f; // Distance to start following the character
    public float stoppingDistance = 2f; // Stopping distance from the character
                                        // Probability of following the character (0 to 1)

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        character = GameObject.Find("Character");
    }

    // Update is called once per frame
    void Update()
    {
        if (character != null)
        {
            float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position);

            if (distanceToCharacter <= followDistance && distanceToCharacter > stoppingDistance)
            {

                Vector3 directionToCharacter = character.transform.position - transform.position;
                Vector3 targetPosition = character.transform.position - directionToCharacter.normalized * stoppingDistance;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);


            }
        }
    }
}
