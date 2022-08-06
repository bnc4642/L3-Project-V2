using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Aarthificial.Reanimation;

public class Player : MonoBehaviour
{
    public float wallRadius;
    public Vector2 wallPos1;
    public Vector2 wallPos2;

    public Rigidbody2D rb;
    public Transform groundCheck;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private GameObject longarrow;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask Enemy;
    [SerializeField] private LayerMask Transition;
    [SerializeField] private InputActionReference actionReference;
    public LevelLoader transitioner;
    public PlayerState State = PlayerState.Movement;


    public Vector2[,] attackPos = new Vector2[2, 3] { { new Vector2(-0.15f, -1.81f), new Vector2(2.02f, 0.03f), new Vector2(-0.12f, 2.36f) }, { new Vector2(0.04f, -1.41f), new Vector2(1.39f, 0.03f), new Vector2(0.16f, 1.15f) } };
    public float[,] attackRadii = new float[2, 3] { { 1.47f, 1.52f, 1.2f }, { 1.28f, 1.52f, 1.76f } };
    public float attackBounce;
    public float hitBounce;
    private Vector2 bounceEffect;
    public float width = 1.15f;
    readonly float DamagePush = 20;
    float speed = 18;
    public int health = 10;
    private int damage = 3;
    float attackingTime = 0;
    float damagedTime = 0;
    private bool hasHitEnemies = false;
    public int attackStyle = 0;
    public int attackingDirection = 0;
    private float dashingPower = 100;
    private float dashingTime = 1.4f;
    public Vector2 direction;
    public Vector3 attackPoint;
    public float radius;


    bool canDash = false;
    bool jumping = false;
    bool falling = false;
    bool interactable = false;
    private bool jumped = false;
    public bool facingRight = true;
    public bool grounded = false;
    public bool walled = false;
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
                try
                {
                    StartCoroutine(EnterAttackState(2));
                }
                catch { }
            }
            else if (context.interaction is HoldInteraction)
            {
                StartCoroutine(EnterAttackState(3));
            }
        };
    }

    public void OnDash(InputValue value)
    {
        EnterDashState();
    }

    public void OnProjectile()
    {
        EnterProjectileState();
    }

    public void OnInteract(InputValue value)
    {
        if (interactable)
            Interact();
    }

    public void OnFloat(InputValue value)
    {
        EnterFloatState();
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

        if (State != PlayerState.Attack && attackingTime < Time.time)
        {
            attackStyle = n;
            State = PlayerState.Attack;

            if (grounded && direction.y < 0)
                attackingDirection = 1;
            else
                attackingDirection = (int)direction.y + 1;

            attackingTime = 0.3f + Time.time;
            if (n == 2)
                attackingTime = 0.25f + Time.time;
        }
        yield return new WaitForSeconds(0);
    }

    private void UpdateAttackState()
    {
        if (attackingTime - 0.4 > Time.time)
            Flip();

        attackPoint = (Vector2)transform.position + attackPos[attackStyle - 2, attackingDirection];
        radius = attackRadii[attackStyle - 2, attackingDirection];

        if (!facingRight)
            attackPoint = new Vector2(transform.position.x - attackPos[attackStyle - 2, attackingDirection].x, transform.position.y + attackPos[attackStyle - 2, attackingDirection].y);

        if (!hasHitEnemies)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, radius, Enemy);

            if (hitEnemies.Length != 0)
                GetComponentInChildren<CameraManager>().TriggerShake(1f, 15f);

            foreach (Collider2D enemy in hitEnemies)
            {
                if (!facingRight && attackingDirection == 1)
                {
                    StartCoroutine(enemy.GetComponent<Enemy>().Hit(damage, 3));
                    AttackBounce(3);
                }
                else
                {
                    StartCoroutine(enemy.GetComponent<Enemy>().Hit(damage, attackingDirection));
                    AttackBounce(attackingDirection);
                }
            }
            hasHitEnemies = true;
        }

        if (!grounded)
            Jump();

        if (attackingTime < Time.time)
        {
            hasHitEnemies = false;
            Flip();
            EnterMovementState();
        }
        bounceEffect.x *= 0.5f;
        bounceEffect.y *= 0.95f;
        rb.velocity = new Vector2(direction.x * speed*0.3f, rb.velocity.y) + bounceEffect;
    }

    private void AttackBounce(int orri)
    {
        switch (orri)
        {
            case 0:
                Debug.Log("Down");
                bounceEffect = new Vector2(0, attackBounce/2);
                break;
            case 1:
                Debug.Log("Right");
                bounceEffect = new Vector2(-attackBounce, 0);
                break;
            case 2:
                Debug.Log("Up");
                bounceEffect = new Vector2(0, -attackBounce / 3);
                break;
            case 3:
                Debug.Log("Left");
                bounceEffect = new Vector2(attackBounce, 0);
                break;
            default:
                break;
        }
    }

    private void EnterMovementState()
    {
        State = PlayerState.Movement;
    }

    private void UpdateMovementState()
    {
        // if(wallColliderLeft && direction.x < 0 || wallColliderRight && direction.x > 0) wallHolding = true;

        if (grounded)
            canDash = true;

        if (dashingTime - 0.5f < Time.time)
        {
            tr.emitting = false;
            rb.gravityScale = 10;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;
           //rb.velocity = new Vector2(0, 0);
        }

        if (bounceEffect.x > 0.05)
            bounceEffect.x *= 0.5f;
        if (bounceEffect.y > 0.05f)
            bounceEffect.y *= 0.6f;

        //walk
        if (dashingTime - 0.5f < Time.time && damagedTime - 0.5 < Time.time) // && !wallHolding
            rb.velocity = new Vector2(direction.x * speed, rb.velocity.y) + bounceEffect;

        walled = IsWalled();

        if (walled && !grounded)
            WallJump();
        else
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
        else if (grounded && falling)
        { // if on ground and finished jumping
            falling = false;
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
        else if (grounded)

        // add a velocity to the right if facing right, or to the left, and prevent player movement for a fraction of a second

        if (jumping && rb.velocity.y > 0) // the longer they wait, the higher they go
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.95f);
        else if (rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        else if (rb.velocity.y <= 0) // tip point
            falling = true;
    }

    private void EnterProjectileState()
    {
        State = PlayerState.Attack;
    }

    private void EnterFloatState()
    {
        State = PlayerState.Movement;
    }

    public bool IsGrounded() { return Physics2D.OverlapBox(groundCheck.position, new Vector2(width, 0.1f), 0, groundLayer); }
    public bool IsWalled() 
    {
        if (Physics2D.OverlapCircle((Vector2)transform.position + wallPos1, wallRadius, groundLayer) || Physics2D.OverlapCircle((Vector2)transform.position + wallPos2, wallRadius, groundLayer))
        {
            Debug.Log("Walled!");
            return true;
        }
        else
            return false;
    }

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

        Gizmos.DrawWireSphere((Vector2)transform.position + wallPos2, wallRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + wallPos1, wallRadius);
        Gizmos.DrawWireSphere(attackPoint, radius);
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(width, 0.1f, 1));
    }
}
