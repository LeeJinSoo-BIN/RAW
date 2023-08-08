using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager2 : MonoBehaviour
{
    public static SkillManager2 instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Skill(object[] _params)
    {
        Vector2 pos = (Vector2)_params[0];
        float duration = (float)_params[1];
        GameObject magic = Instantiate((GameObject) _params[2]);
        magic.transform.position = pos;
        StartCoroutine(Vanish(duration, magic));
    }

    IEnumerator Vanish(float duration, GameObject who)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        who.GetComponent<Animator>().SetTrigger("vanish");
        time = 0;
        while (time < 0.45)
        {
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(who);
    }
}