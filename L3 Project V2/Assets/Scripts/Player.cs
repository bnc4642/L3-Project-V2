using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Aarthificial.Reanimation;

public class Player : MonoBehaviour
{

    public Rigidbody2D rb;
    public Transform groundCheck;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private GameObject longarrow;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask Enemy;
    [SerializeField] private LayerMask Transition;
    readonly float DamagePush = 20;
    public LevelLoader transitioner;
    public float width = 1.15f;

    float arrowSpeed = 50;
    float speed = 18;

    bool jumping = false;
    bool falling = false;
    bool interactable = false;
    bool attacking = false;
    float attackingTime = 0;
    float damagedTime = 0;
    public bool pogoFalling = false;
    private bool jumped = false;

    public Vector2 direction;
    public PlayerState State = PlayerState.Movement;
    public bool grounded = false;
    public bool facingRight = true;

    public Transform attackPoint;
    public float radius;
    public int damage;

    bool canDash = true;
    private float dashingPower = 100;
    private float dashingTime = 1.4f;
    public int health = 10;

    public void OnMove(InputValue value)
    {
        direction = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        jumping = value.Get<float>() > 0.5f;
        if (!jumping)
            jumped = false;
    }
    
    public void OnAttack(InputValue value)
    {
        EnterAttackState();
    }

    public void OnDash(InputValue value)
    {
        EnterDashState();
    }

    public void OnThrow(InputValue value)
    {
        GameObject a = Instantiate(longarrow);  // shoot arrow

        if (facingRight)
        {
            a.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.2f, 0);
            a.GetComponent<Rigidbody2D>().velocity = new Vector2(arrowSpeed, 0);
            a.GetComponent<Projectile>().FacingRight = true;
        }
        else
        {
            a.transform.localScale = new Vector3(-3, 3, 1);
            a.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.2f, 0);
            a.GetComponent<Rigidbody2D>().velocity = new Vector2(-arrowSpeed, 0);
        }
    }

    public void OnShield(InputValue value)
    {

    }

    public void OnInteract(InputValue value)
    {
        if (interactable)
            Debug.Log("Interact");
    }

    public void OnMagic(InputValue value)
    {

    }

    private void FixedUpdate()
    {
        grounded = IsGrounded();
        interactable = Physics2D.OverlapCircle(transform.position, 1, LayerMask.NameToLayer("Interactable"));

        switch (State)
        {
            case PlayerState.Movement:
                Flip();
                UpdateMovementState();
                break;
            case PlayerState.Attack:
                UpdateAttackState();
                break;
            case PlayerState.Hit:
                UpdateHitState();
                break;
            case PlayerState.Dash:
                UpdateDashState();
                break;
        }
    }

    private void EnterDashState()
    {
        if (!canDash || dashingTime > Time.time) return;
        State = PlayerState.Dash;

        canDash = false;
        rb.gravityScale = 0;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);
        tr.emitting = true;
        dashingTime = Time.time + .6f;
    }

    private void UpdateDashState()
    {
        if (dashingTime - 0.5f > Time.time) return;

        tr.emitting = false;
        rb.gravityScale = 10;
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;

        if (attacking)
        {
            State = PlayerState.Attack;
            EnterAttackState();
            attacking = false;
            return;
        }

        EnterMovementState();
    }

    public void EnterHitState(Collision2D collision, int dmg)
    {
        if (State != PlayerState.Hit && damagedTime > Time.time) return;
        State = PlayerState.Hit;

        damagedTime = Time.time + 0.8f;
        transitioner.transition.SetTrigger("Hurt");
        if (collision.gameObject.transform.position.x < transform.position.x)
            rb.velocity = new Vector2(DamagePush, DamagePush / 3);
        else
            rb.velocity = new Vector2(-DamagePush, DamagePush / 3);

        GetComponentInChildren<CameraManager>().TriggerShake(1.5f, 10f);
        health -= dmg;
        if (health <= 0)
            Die();
    }

    private void UpdateHitState()
    {
        if (damagedTime - 0.5 < Time.time)
            EnterMovementState();
    }

    private void EnterAttackState()
    {
        if (State == PlayerState.Dash)
            attacking = true;

        if (!attacking && (State == PlayerState.Attack || State == PlayerState.Dash || attackingTime > Time.time)) return;
        else if (attacking && State == PlayerState.Dash) return;

        State = PlayerState.Attack;

        if (!grounded && direction.y < 0)
        {
            attackingTime = 0.15f + Time.time;
            rb.velocity = new Vector2(0, rb.velocity.y);
            pogoFalling = true;
            GetComponent<Reanimator>().Set("swordFallAnim", 0);
        }
        else if (direction.y == 0 || (grounded && direction.y < 0))
            attackingTime = 0.5f + Time.time;
    }

    private void UpdateAttackState()
    {
        if (attackingTime - 0.4 > Time.time)
            Flip();
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, radius, Enemy);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.GetComponent<Enemy>() != null)
            {
                if (pogoFalling)
                    StartCoroutine(enemy.GetComponent<Mosquito>().Hit(damage, 2));
                else
                    StartCoroutine(enemy.GetComponent<Mosquito>().Hit(damage, Convert.ToInt32(facingRight)));
            }
        }

        if (!pogoFalling)
        {
            if (!grounded)
                Jump();

            if (attackingTime < Time.time)
            {
                Flip();
                EnterMovementState();
            }
            rb.velocity = new Vector2(direction.x * speed*0.3f, rb.velocity.y);
        }
        else if (grounded)
        {
            pogoFalling = false;
            GetComponent<Reanimator>().Set("idleTransition", 0);
            EnterMovementState();
        }
    }

    private void EnterMovementState()
    {
        State = PlayerState.Movement;
    }

    private void UpdateMovementState()
    {
        //walk
        if (State != PlayerState.Dash && damagedTime - 0.5 < Time.time)
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        Jump();
    }

    private void Jump()
    {
        //jump
        if (!jumped && jumping && grounded)
        {
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 40);
        }
        if (grounded && falling)
        { // if on ground and finished jumping
            falling = false;
            canDash = true;
        }

        if (jumping && rb.velocity.y > 0) // the longer they wait, the higher they go
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.95f);
        else if (rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        else if (rb.velocity.y <= 0) // tip point
            falling = true;
    }

    public bool IsGrounded() { return Physics2D.OverlapBox(groundCheck.position, new Vector2(width, 0.1f), 0, groundLayer); }

    private void Flip()
    {
        if (facingRight && direction.x < 0 || !facingRight && direction.x > 0)
        {
            facingRight = !facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & Transition) != 0)
        {
            // prepare any possible cutscenes
            // save player data and input it into next scene
            // output player into the correct location, and make them walk
            // (make the corrosponding transitioners have the same name, so you can check for the right one, and output them correctly).
            StartCoroutine(transitioner.LoadLevel(collision.gameObject.GetComponent<Transitioner>().nextScene));
        }
    }

    public void Die()
    {
        Debug.Log("Die");
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, radius);
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(width, 0.1f, 1));
    }
}
