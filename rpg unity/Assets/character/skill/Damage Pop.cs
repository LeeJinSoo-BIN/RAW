using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePop : MonoBehaviour
{
    // Start is called before the first frame update

    public float upSpeed;
    public float alphaSpeed;
    public float disappeadTime;
    public float smallSpeed;
    public TMP_Text text;
    
    private Color color;
    // Update is called once per frame
    private void Start()
    {
        color = text.color;
        Destroy(gameObject, disappeadTime);
    }
    void Update()
    {
        transform.Translate(Vector2.up * Time.deltaTime * upSpeed);
        transform.localScale = transform.localScale * (1 / (1 + Time.deltaTime * smallSpeed));
        color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * alphaSpeed);
        text.color = color;
    }
}
