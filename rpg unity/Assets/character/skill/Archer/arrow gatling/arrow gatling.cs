using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class arrowgatling : MonoBehaviour
{
    public Vector3 targetPos;
    public float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = targetPos - transform.position;
        if(dir.magnitude < 0.01f)
            Destroy(gameObject);

        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed);
        //transform.position += dir * speed * Time.deltaTime;


        float angle_pi = Mathf.Atan2(transform.position.y - targetPos.y, transform.position.x - targetPos.x);
        float angle_rad = angle_pi * Mathf.Rad2Deg + 180;
        /*if (transform.localScale.x > 0)
            angle_rad -= 180;*/
        transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);


        
        
    }
}
