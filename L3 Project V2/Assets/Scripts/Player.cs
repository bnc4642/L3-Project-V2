using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float s;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator anim;

    public float speed = 8;
    public float jumpingPower = 20;
    public float bowTime;
    public GameObject longarrow;
    public float arrowSpeed;
    float horizontal;
    bool facingRight = true;
    bool Jumping = false;
    float time = 0;
    bool Drawing = false;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.C) && Drawing)
        {
            if (time + bowTime <= Time.time)    // arrow shoot
            {
                GameObject a = Instantiate(longarrow);
                if (facingRight)
                {
                    a.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.2f, 0);
                    a.GetComponent<Rigidbody2D>().velocity = new Vector2(arrowSpeed, 0);
                }
                else
                {
                    a.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.2f, 0);
                    a.GetComponent<Rigidbody2D>().velocity = new Vector2(-arrowSpeed, 0);
                }
                Drawing = false;
                anim.SetBool("Drawing", false);
            }
            else    // still drawing
                return;
        }
        else if (Input.GetKeyUp(KeyCode.C) && Drawing)
        {
            Drawing = false;
            anim.SetBool("Drawing", false);
        }

        else if (IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                time = Time.time;
                Drawing = true;
                anim.SetBool("Drawing", true);
                return;
            }
            if (Jumping)
            {
                anim.SetBool("Jumping", false);
                Jumping = false;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
                StartCoroutine("Jump");
        }
        else
            if (anim.GetBool("Jumping"))
                Jumping = true;
        if (Input.GetKeyUp(KeyCode.Z) && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

        Flip();
    }

    private void FixedUpdate()
    {
        if (Drawing)
            horizontal = 0;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        if (horizontal != 0)
            anim.SetBool("Walking", true);
        else
            anim.SetBool("Walking", false);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void Flip()
    {
        if (facingRight && horizontal < 0 || !facingRight && horizontal > 0)
        {
            facingRight = !facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Jump()
    {
        anim.SetBool("Jumping", true);
        yield return new WaitForSeconds(s);
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
    }
}
