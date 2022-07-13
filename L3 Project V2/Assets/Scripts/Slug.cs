using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slug : MonoBehaviour
{
    public Rigidbody2D rb;
    float xSpeed = 2;
    float ySpeed;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Transform sideCheck;

    int orrientation = 0;

    void Update()
    {
        rb.velocity = new Vector2(xSpeed, ySpeed);

        if (!Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer))
        {
            transform.rotation = Quaternion.Euler(0, 0, (-orrientation - 1) * 90);
            if (orrientation == 0 || orrientation == 2)
            {
                xSpeed = 0;
                ySpeed = (orrientation - 1) * 2;
                orrientation++;
            }
            else
            {
                ySpeed = 0;
                xSpeed = (orrientation - 2) * 2;
                if (orrientation == 3)
                    orrientation = 0;
                else
                    orrientation++;
            }
        }
        else if (Physics2D.OverlapCircle(sideCheck.position, 0.2f, groundLayer)) //Broken
        {
            Quaternion rotation = transform.rotation;
            rotation *= Quaternion.Euler(0, 0, 90);
            transform.rotation = rotation;

            if (orrientation == 0 || orrientation == 2)
            {
                xSpeed = 0;
                ySpeed = -(orrientation - 1) * 2;
            }
            else
            {
                ySpeed = 0;
                xSpeed = -(orrientation - 2) * 2;
            }

            if (orrientation == 0)
                orrientation = 3;
            else
                orrientation--;
        }
    }
}
