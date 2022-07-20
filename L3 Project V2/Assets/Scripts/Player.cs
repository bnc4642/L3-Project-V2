using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator anim;
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
    float jumpingPower = 35;
    float horizontal;
    bool facingRight = true;
    float time = 0;
    bool Jumping = false;
    bool Falling = false;
    bool Drawing = false;
    bool Stabbing = false;
    bool damaged = false;
    public bool pogoFalling = false;
    bool pogoStabbing = false;

    public Transform attackPoint;
    public float radius;
    public int damage;

    bool canDash = true;
    private bool isDashing;
    private float dashingPower = 40;
    private float dashingTime = 0.15f;
    private float dashingCooldown = 0.2f;
    public int health = 10;

    private static readonly int Idle = Animator.StringToHash("PlayerIdle3");
    private static readonly int Walk = Animator.StringToHash("PlayerRun2");
    private static readonly int Jump = Animator.StringToHash("PlayerJumpFirst");
    private static readonly int Fall = Animator.StringToHash("PlayerFall2");
    private static readonly int Draw = Animator.StringToHash("PlayerLongbow");
    private static readonly int Stab = Animator.StringToHash("PlayerStab3");
    private static readonly int pogoFall = Animator.StringToHash("PlayerPogoFall");
    private static readonly int pogoStab = Animator.StringToHash("PlayerPogoStab");
    //private static readonly int Land = Animator.StringToHash("Land");
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
                if (pogoFalling)
                    StartCoroutine(ToPogoStab(0));
            }

            //knife
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (!Stabbing)
                    StartCoroutine(ToStab());
            }

            if (Input.GetKeyDown(KeyCode.C))
            { // arrow business
                lockedTill = Time.time + 0.5f;
                Drawing = true;
            }
        }
        else // if not on ground
        {
            if (rb.velocity.y <= 0) // tip point
            {
                Falling = true;
                Jumping = false;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                pogoFalling = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.Z) && rb.velocity.y > 0) // the longer they wait, the higher they go
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

        

        if (Stabbing || pogoFalling)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, radius, Enemy);

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.GetComponent<Mosquito>() != null)
                {
                    if (pogoFalling)
                    {
                        StartCoroutine(ToPogoStab(1));
                        StartCoroutine(enemy.GetComponent<Mosquito>().Hit(damage, 2));
                    }
                    else
                        StartCoroutine(enemy.GetComponent<Mosquito>().Hit(damage, Convert.ToInt32(facingRight)));
                }
            }
        }


        //shoot
        if (Input.GetKey(KeyCode.C) && Drawing && time + 0.5f <= Time.time) // if holding arrow button
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

            Drawing = false;
        }
        else if (Input.GetKeyUp(KeyCode.C)) // if released arrow button
        {
            Drawing = false;    // stop drawing
            lockedTill = 0;
        }

        
        //dash
        if (isDashing)
            return;
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine("Dash");


        if (!Stabbing && !Drawing)
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
        if (Drawing || Stabbing || pogoFalling)
            horizontal = 0;
        if (!(isDashing || damaged))
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private int GetState()
    {
        //use a switch case ( maybe just use simple values for anims? idk)


        if (Time.time < lockedTill) return currentState;
        // Priorities

        if (damaged && Time.time < lockedTill + 0.3)
            damaged = false;
        if (pogoFalling)
            return pogoFall;
        if (pogoStabbing)
            return pogoStab;
        if (Stabbing)
            return Stab;
        if (Drawing)
        {
            return Draw;
        }
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

        //int LockState(int s, float t)
        //{
            //lockedTill = Time.time + t;
            //return s;
        //}
    }

    private IEnumerator ToJump()
    {
        Jumping = true;
        yield return new WaitForSeconds(0.05f);
        rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
    }

    private IEnumerator ToStab()
    {
        Stabbing = true;
        if (facingRight)
            rb.velocity = new Vector2(-3, rb.velocity.y);
        yield return new WaitForSeconds(0.3f);
        Stabbing = false;
    }

    private IEnumerator ToPogoStab(int enemyStab)
    {
        pogoFalling = false;
        pogoStabbing = true;
        rb.velocity = new Vector2(enemyStab * 30 * Input.GetAxisRaw("Horizontal"), enemyStab * 40);
        yield return new WaitForSeconds(0.3f);
        pogoStabbing = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);
        tr.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;
        rb.gravityScale = originalGravity;
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);

        if (!Jumping && !Falling)
            canDash = true;
    }

    private bool IsGrounded() { return Physics2D.OverlapBox(groundCheck.position, new Vector2(width, 0.1f), 0, groundLayer); }

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

    public void Hit(GameObject enemy, int dmg)
    {
        if (!damaged)
        {
            damaged = true;
            lockedTill = Time.time + 0.2f;
            transitioner.transition.SetTrigger("Hurt");
            if (enemy.gameObject.transform.position.x < transform.position.x)
                rb.velocity = new Vector2(DamagePush, DamagePush / 3);
            else
                rb.velocity = new Vector2(-DamagePush, DamagePush / 3);

            GetComponentInChildren<CameraManager>().TriggerShake(1.5f, 10f);
            health -= dmg;
            if (health <= 0)
                Die();
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
