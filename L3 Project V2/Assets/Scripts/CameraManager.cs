using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //variables
    private Transform cam;
    private float shakeTime = 0;
    private float shakeAmount = 0.2f;
    private float damping = 10;
    private Vector3 initPos;
    private Vector3 velocity = new Vector3(5, 5, 5);

    private void Awake()
    {
        //set local scene variable
        cam = GameObject.Find("NewPixelCamera 1").transform;
    }

    void Update()
    {
        //make sure camera doesn't display outside of level boundaries
        Vector3 targetPosition = transform.position;
        targetPosition.x = Mathf.Clamp(targetPosition.x, GM.Instance.xToY[0], GM.Instance.xToY[1]);
        targetPosition.y = Mathf.Clamp(targetPosition.y, GM.Instance.xToY[2], GM.Instance.xToY[3]);
        //smoothly move towards player
        cam.position = Vector3.SmoothDamp(cam.position, targetPosition, ref velocity, 0.25f);

        //if camera shake is activated
        if (shakeTime > 0)
        {
            //move the camera about randomly to shake
            cam.localPosition = initPos + Random.insideUnitSphere * shakeAmount;
            shakeTime -= Time.deltaTime * damping; //custom timing
        }
    }

    public void TriggerShake(float t, float d)
    {
        damping = d;
        //initiates shake
        shakeTime = t;
        initPos = cam.localPosition;
    }
}
