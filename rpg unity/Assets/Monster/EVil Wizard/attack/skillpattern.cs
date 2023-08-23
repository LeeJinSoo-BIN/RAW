using JetBrains.Annotations;
using UnityEngine;

public class skillIpattern : MonoBehaviour
{
    private MobControl mobControl;
    private skill_2 skill2;
    private skill_3 skill3;
    public GameObject fireprefab;
    private float elapsedTime = 0.0f;
    private bool skillInvoked = false;
    private bool mobControlEnabled = true; // MobControl Ȱ��ȭ ���θ� �����ϴ� ����

    private void Start()
    {
        mobControl = GetComponent<MobControl>();
        skill2 = GetComponent<skill_2>();
        skill3 = GetComponent<skill_3>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (!skillInvoked && elapsedTime >= 30.0f) // 30�ʰ� ���� �� ��ų ���
        {
            skillInvoked = true;
            DisableMobControl(); // MobControl�� ��Ȱ��ȭ
            InvokeRandomSkillsContinuously();
        }
    }

    private void DisableMobControl()
    {
        if (mobControlEnabled)
        {
            mobControl.enabled = false; // MobControl ��Ȱ��ȭ
            mobControlEnabled = false;
        }
    }

    private void InvokeRandomSkillsContinuously()
    {
        StartCoroutine(InvokeRandomSkillsRoutine());
    }

    private void MakeFire()
    {
        GameObject fire = Instantiate(fireprefab, transform.position, Quaternion.identity);

        // ��ų1 Ŭ������ �ν��Ͻ��� ����� ����
        skill_1 skill1 = fire.AddComponent<skill_1>();
        skill1.monster = gameObject;
        skill1.character = mobControl.character;
        skill1.ExecuteSkill();
    }

    private System.Collections.IEnumerator InvokeRandomSkillsRoutine()
    {
        while (true)
        {
            int randomSkill = Random.Range(1, 4);
            switch (randomSkill)
            {
                case 1:
                    MakeFire();
                    break;
                case 2:
                    skill2.ExecuteSkill();
                    break;
                case 3:
                    skill3.ExecuteSkill();
                    break;
            }

            yield return new WaitForSeconds(3.0f); // ��ų ȣ�� ���� (��)
        }
    }
}
