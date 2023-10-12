using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFollow : MonoBehaviour
{

    public Vector3 cameraPosition = new Vector3(0, 0, -10);
    public Transform myCharacterTransform;
    public float cameraMoveSpeed;
    public Vector2 mapSize;
    public Vector2 center;
    float height;
    float width;

    private void Start()
    {
        Vector2 leftTop = GameObject.Find("map").transform.Find("left top").position;
        Vector2 rightBottom = GameObject.Find("map").transform.Find("right bottom").position;        
        mapSize = new Vector2(Mathf.Abs(rightBottom.x - leftTop.x)/2, Mathf.Abs(leftTop.y - rightBottom.y)/2);
        Debug.Log(mapSize);
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;
    }
    // Update is called once per frame
    void Update()
    {
        LimitCameraArea();
        
    }

    void LimitCameraArea()
    {
        transform.position = Vector3.Lerp(transform.position,
                                          myCharacterTransform.position + cameraPosition,
                                          Time.deltaTime * cameraMoveSpeed);
        float lx = mapSize.x - width;
        float clampX = Mathf.Clamp(transform.position.x, -lx + center.x, lx + center.x);

        float ly = mapSize.y - height;
        float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

        transform.position = new Vector3(clampX, clampY, -10f);
    }
}
