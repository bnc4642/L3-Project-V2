using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    Vector3 offset = new Vector3(0, 0, -10);
    float smoothTime = 0.25f;
    Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform target;

    void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
