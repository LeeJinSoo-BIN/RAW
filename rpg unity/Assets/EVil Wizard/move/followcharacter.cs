using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobControl : MonoBehaviour
{
    public GameObject character; // ĳ���� GameObject�� ���� ����
    public float moveSpeed = 3f; // ���� �̵� �ӵ�
    public float followDistance = 5f; // ĳ���͸� ���󰡱� ������ �Ÿ�
    public float stoppingDistance = 2f; // ���߱� ���� �Ÿ�

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ���� Rigidbody2D ������Ʈ ��������
        character = GameObject.Find("Character"); // ������ "Character"��� �̸��� ���� ĳ���� GameObject ã��
    }

    // Update is called once per frame
    void Update()
    {
        if (character != null) // ĳ���� GameObject ������ null�� �ƴ��� Ȯ�� (ĳ���Ͱ� ���� �����ϴ���)
        {
            float distanceToCharacter = Vector3.Distance(transform.position, character.transform.position); // ���� ĳ���� ������ �Ÿ� ���

            if (distanceToCharacter <= followDistance && distanceToCharacter > stoppingDistance) // ĳ���Ϳ��� �Ÿ��� followDistance �̳��̸鼭 stoppingDistance���� ũ�ٸ�
            {
                Vector3 targetPosition = new Vector3(character.transform.position.x, character.transform.position.y, transform.position.z); // ���� ������ z���� �����鼭 ĳ������ x�� y ��ġ�� ���� targetPosition ����
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime); // Time.deltaTime�� ����Ͽ� moveSpeed�� ���� �ӵ��� targetPosition���� �̵�
            }
        }
    }
}
