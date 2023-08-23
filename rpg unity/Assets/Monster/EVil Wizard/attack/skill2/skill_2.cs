using UnityEngine;

public class skill_2 : MonoBehaviour
{
    public GameObject monster;       // ���� GameObject
    public GameObject character;     // ĳ���� GameObject
    public float moveSpeed = 5.0f;   // �̵� �ӵ�

    private Vector3 targetPosition;  // �̵��� ��ǥ ��ġ
    private bool isMoving = false;   // �̵� �� ����

    public void ExecuteSkill()
    {
        // ������ ���� ��ġ�� ĳ���� ��ó�� ����
        targetPosition = character.transform.position;
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            // ���� ��ġ���� ��ǥ ��ġ�� �̵�
            float step = moveSpeed * Time.deltaTime;
            monster.transform.position = Vector3.MoveTowards(monster.transform.position, targetPosition, step);

            // ��ǥ ��ġ�� �����ϸ� �̵� ����
            if (monster.transform.position == targetPosition)
            {
                isMoving = false;
            }
        }
    }
}
