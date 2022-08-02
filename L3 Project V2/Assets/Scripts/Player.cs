using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
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
    [SerializeField] private InputActionReference actionReference;
    readonly float DamagePush = 20;
    public LevelLoader transitioner;
    public float width = 1.15f;

    float arrowSpeed = 50;
    float speed = 18;

    bool jumping = false;
    bool falling = false;
    bool interactable = false;
    float attackingTime = 0;
    float damagedTime = 0;
    public bool pogoFalling = false;
    private bool jumped = false;

    public int attackStyle = 0;
    public int attackingDirection = 0;
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
    
    public void OnStab(InputValue value)
    {
        actionReference.action.performed += context =>
        {
            if (context.interaction is TapInteraction)
            {
                Debug.Log("This");
            }
        };
        EnterAttackState(2);
    }

    public void OnSlash(InputValue value)
    {
        actionReference.action.performed += context =>
        {
            if (context.interaction is HoldInteraction)
            {
                Debug.Log("That");
            }
        };
        EnterAttackState(3);
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
            Interact();
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
        }
    }

    private void EnterDashState()
    {
        if (!canDash || dashingTime > Time.time) return;

        canDash = false;
        rb.gravityScale = 0;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);
        tr.emitting = true;
        dashingTime = Time.time + .6f;
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
        {
            EnterMovementState();
        }
    }

    private IEnumerator EnterAttackState(int n)
    {
        if (dashingTime > Time.time)
            yield return new WaitForSeconds(dashingTime - Time.time);

        if (State != PlayerState.Attack && dashingTime < Time.time && attackingTime < Time.time)
        {

            attackStyle = n;

            switch (direction.y)
            {
                case > 0:
                    attackingDirection = 2;
                    AttackDirection(2);
                    break;
                case < 0:
                    if (grounded)
                        AttackDirection(1);
                    else
                        AttackDirection(0);
                    break;
                default:
                    attackingDirection = 1;
                    AttackDirection(1);
                    break;
            }

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
        yield return null;
    }

    private void AttackDirection(int n)
    {
        if (n == 0)
        {
            attackPoint.position = new Vector3();
            radius = 2;
        }
        else if (n == 1)
        {
            attackPoint.position = new Vector3();
            radius = 2;
        }
        else if (n == 2)
        {
            attackPoint.position = new Vector3();
            radius = 2;
        }
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

        if (!grounded)
            Jump();

        if (attackingTime < Time.time)
        {
            Flip();
            EnterMovementState();
        }
        rb.velocity = new Vector2(direction.x * speed*0.3f, rb.velocity.y);
    }

    private void EnterMovementState()
    {
        State = PlayerState.Movement;
    }

    private void UpdateMovementState()
    {
        // if(wallColliderLeft && direction.x < 0 || wallColliderRight && direction.x > 0) wallHolding = true;

        if (dashingTime - 0.5f < Time.time)
        {
            tr.emitting = false;
            rb.gravityScale = 10;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;
           //rb.velocity = new Vector2(0, 0);
        }

        //walk
        if (dashingTime - 0.5f < Time.time && damagedTime - 0.5 < Time.time) // && !wallHolding
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        // if (wallHolding && !grounded)
        // WallJump()
        // else
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

    private void WallJump()
    {
        //jump
        if (!jumped && jumping) // && wallHolding
        {
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 40);
        }
        if (grounded && falling)
        { // if on ground and finished jumping
            falling = false;
            canDash = true;
        }

        // add a velocity to the right if facing right, or to the left, and prevent player movement for a fraction of a second

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

    private  void Interact()
    {

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
