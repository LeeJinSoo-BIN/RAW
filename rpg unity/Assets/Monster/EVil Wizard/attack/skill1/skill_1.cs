using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class skill_1 : MonoBehaviour
{
    public GameObject character;
    public GameObject firePrefab; // �� ������
    public Transform throwPoint;  // ���� ���� ��ġ

    public void ExecuteSkill()
    {
        // ĳ������ ���� ��ġ�� ���� ������ ����
        Vector3 characterPosition = character.transform.position;
        Instantiate(firePrefab, characterPosition, Quaternion.identity);
    }
}


