using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-3, 3), Random.Range(2, 4));
    }
}
