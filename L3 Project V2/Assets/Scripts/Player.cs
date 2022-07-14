using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator anim;
    [SerializeField] private TrailRenderer tr;
    public GameObject longarrow;

    float arrowSpeed = 50;
    float speed = 8;
    float jumpingPower = 20;
    float horizontal;
    bool facingRight = true;
    float time = 0;
    bool Jumping = false;
    bool Falling = false;
    bool Drawing = false;

    bool canDash = true;
    private bool isDashing;
    private float dashingPower = 12;
    private float dashingTime = 0.15f;
    private float dashingCooldown = 0.3f;

    private static readonly int Idle = Animator.StringToHash("PlayerIdle2");
    private static readonly int Walk = Animator.StringToHash("PlayerWalk");
    private static readonly int Jump = Animator.StringToHash("PlayerJumpUp");
    private static readonly int Fall = Animator.StringToHash("PlayerFall");
    private static readonly int Land = Animator.StringToHash("Land");
    private static readonly int Draw = Animator.StringToHash("PlayerLongbow");
    //private static readonly int Crouch = Animator.StringToHash("Crouch");

    private float lockedTill = 0;
    private int currentState;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        //Current abilities.
        // walk, dash, jump, shoot

        //jump
        if (IsGrounded())
        {
            if (Input.GetKeyDown(KeyCode.Z)) // if on ground, then jump
                StartCoroutine("ToJump");
            if (Falling)
            { // if on ground and finished jumping
                Falling = false;
                canDash = true;
            }


            if (Input.GetKeyDown(KeyCode.C))
            { // arrow business
                time = Time.time;
                Drawing = true;
            }


        }
        else // if not on ground, must be jumping
        {
            if (rb.velocity.y <= 0)
            {
                Falling = true;
                Jumping = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.Z) && rb.velocity.y > 0) // the longer they wait, the higher they go
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);


        //shoot
        if (Input.GetKey(KeyCode.C) && Drawing && time + 0.5f <= Time.time) // if holding arrow button
        {
            GameObject a = Instantiate(longarrow);  // shoot arrow

            if (facingRight)
            {
                a.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.2f, 0);
                a.GetComponent<Rigidbody2D>().velocity = new Vector2(arrowSpeed, 0);
            }
            else
            {
                a.transform.localScale = new Vector3(-3, 3, 1);
                a.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.2f, 0);
                a.GetComponent<Rigidbody2D>().velocity = new Vector2(-arrowSpeed, 0);
            }
                
            Drawing = false;
        }
        else if (Input.GetKeyUp(KeyCode.C)) // if released arrow button
            Drawing = false;    // stop drawing

        
        //dash
        if (isDashing)
            return;
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine("Dash");


        Flip();

        //animation
        var state = GetState();

        if (state == currentState) return;
        anim.CrossFade(state, 0, 0);
        currentState = state;
    }

    private void FixedUpdate()
    {
        //walk
        if (Drawing)
            horizontal = 0;
        if (isDashing) { }
        else
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private int GetState()
    {
        if (Time.time < lockedTill) return currentState;
        // Priorities
        if (Drawing)
            return Draw;
        //if (crouching) return Crouch;
        if (Jumping) return Jump;
        if (Falling) return Fall;

        if (IsGrounded())
        {
            if (horizontal == 0)
                return Idle;
            else return Walk;
        }

        return rb.velocity.y > 0 ? Jump : Fall;

        int LockState(int s, float t)
        {
            lockedTill = Time.time + t;
            return s;
        }
    }

    private IEnumerator Knife()
    {
        yield return new WaitForSeconds(2);
    }

    private IEnumerator ToJump()
    {
        Jumping = true;
        yield return new WaitForSeconds(0.05f);
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        if (!Jumping && !Falling)
            canDash = true;
    }

    private bool IsGrounded() { return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer); }

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
}
