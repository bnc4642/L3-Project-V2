using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollUV : MonoBehaviour
{
    public Transform cam;
    public float Speed = 1;

    float length;

    Vector2 physicalOffset;
    void Start()
    {
        physicalOffset = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float temp = cam.position.x * (1 - Speed);
        Vector2 offset = cam.position * Speed;

        transform.position = physicalOffset + offset;

        if (temp > physicalOffset.x + length) physicalOffset.x += length;
        else if (temp < physicalOffset.x - length) physicalOffset.x -= length;
    }
}
