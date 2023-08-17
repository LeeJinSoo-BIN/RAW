using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class skill_1 : MonoBehaviour
{
    public GameObject character;
    public GameObject firePrefab; // 불 프리팹
    public Transform throwPoint;  // 불을 던질 위치

    public void ExecuteSkill()
    {
        // 캐릭터의 현재 위치에 불을 던지는 로직
        Vector3 characterPosition = character.transform.position;
        Instantiate(firePrefab, characterPosition, Quaternion.identity);
    }
}


