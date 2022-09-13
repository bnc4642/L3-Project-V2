using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    public Transform cam;

    public float shakeTime = 0;
    public float shakeAmount = 0.7f;
    public float damping = 1;
    private Vector3 initPos;
    Vector3 velocity = new Vector3(5, 5, 5);

    private void Start()
    {
    }

    void Update()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, 0, -10);
        targetPosition.x = Mathf.Clamp(targetPosition.x, xMin, xMax);
        targetPosition.y = Mathf.Clamp(targetPosition.y, yMin, yMax);
        cam.position = Vector3.SmoothDamp(cam.position, targetPosition, ref velocity, 0.25f);

        if (shakeTime > 0)
        {
            cam.localPosition = initPos + Random.insideUnitSphere * shakeAmount;
            shakeTime -= Time.deltaTime * damping;
        }
    }

    public void TriggerShake(float t, float d)
    {
        damping = d;
        shakeTime = t;
        initPos = cam.localPosition;
    }

    public float cameraSpeed;
}
