using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPos : MonoBehaviour
{
    Vector3 offset = new Vector3(0, 0, -10);
    float smoothTime = 0.25f;
    Vector3 velocity = Vector3.zero;

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    [SerializeField] private Transform target;


    void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, xMin, xMax);
        targetPosition.y = Mathf.Clamp(targetPosition.y, yMin, yMax);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
