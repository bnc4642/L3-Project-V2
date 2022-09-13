using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using Aarthificial.Reanimation;
using UnityEngine.VFX;

public class Player : MonoBehaviour
{
    public GameObject healthBar;
    public Vector3 posOffset;
    public Animator deathAnim;
    public List<ParticleSystem> Trails = new List<ParticleSystem>();
    public GameObject deathFX;
    public GameObject impactPrefab;
    public SpriteRenderer energyOrb;
    public Sprite[] orbs = new Sprite[9];

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

    public GameObject[] attackVFX = new GameObject[2];
    public Vector2[,] attackPos = new Vector2[2, 3] { { new Vector2(0.04f, -1.41f), new Vector2(2.02f, 0.03f), new Vector2(-0.12f, 2.36f) }, { new Vector2(0.04f, -1.41f), new Vector2(1.39f, 0.03f), new Vector2(0.16f, 1.15f) } };
    float[] attackRadii = new float[2] { 2.55f , 2.55f };
    public float attackBounce;
    public float hitBounce;
    private Vector2 bounceEffect;
    public float width = 1.15f;
    readonly float DamagePush = 20;
    float speed = 18;
    public int health = 5;
    private int damage = 3;
    float attackingTime = 0;
    float damagedTime = 0;
    public int attackStyle = 0;
    public int attackingDirection = 0;
    private float dashingPower = 100;
    private float dashingTime = 1.4f;
    public Vector2 direction;
    public Vector3 attackPoint;
    public float radius;
    private float lSpeedMult = 1;
    private float rSpeedMult = 1;

    private int energyLevel = 0;
    public float mourningPeriod;
    public float healingTime = 0;
    public bool healing = false;
    public bool healCancelled = false;
    public bool stoppedHealing = true;
    bool canDash = false;
    bool jumping = false;
    bool falling = false;
    public Interactable interactable;
    private bool jumped = false;
    public bool facingRight = true;
    public bool grounded = false;
    public bool walled = false;
    public bool floating = false;

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
        StartCoroutine(EnterAttackState(3));
    }

    public void OnDash(InputValue value)
    {
        EnterDashState();
    }

    public void OnInteract(InputValue value)
    {
        if (interactable != null)
            Interact();
    }

    public void OnFloat(InputValue value)
    {
        if (value.Get<float>() > 0.5f && (attackStyle != 2 || attackingTime < Time.time))
        {
            floating = true;
            rb.gravityScale = 0;
        }
        else
        {
            floating = false;
            rb.gravityScale = 10;
        }
    }

    public void OnSlam(InputValue value)
    {
        StartCoroutine(EnterAttackState(2));
    }

    public void OnHeal(InputValue value)
    {
        healing = value.Get<float>() > 0.5f && State == PlayerState.Movement && grounded;

        if (stoppedHealing && healing && energyLevel > 2) // Is true after pressing button down
        {
            healingTime = Time.time + 1;
            healCancelled = false;
            rb.velocity = Vector2.zero;
        }

        if (!healing)
            stoppedHealing = true;
        else
            stoppedHealing = false;
    }

    private void FixedUpdate()
    {
        if (damagedTime - 0.65 < Time.time) // if hurt and attacking
        {
            GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
            GetComponent<SpriteRenderer>().color = Color.white;
        }

        grounded = IsGrounded();
        Debug.Log(interactable == null);
        //interactable = Physics2D.OverlapCircle(transform.position, 1, LayerMask.NameToLayer("Interactable"));

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
            case PlayerState.Death:
                break;
        }

        if (floating || (walled && !grounded && !jumping))
            rb.velocity = new Vector2(rb.velocity.x, 0);
            
    }

    private void EnterDashState()
    {
        if (!canDash || dashingTime > Time.time || healing) return;

        canDash = false;
        rb.gravityScale = 0;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = true;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);
        tr.emitting = true;
        dashingTime = Time.time + 0.6f;
    }

    public void EnterHitState(GameObject collider, int dmg)
    {
        if (damagedTime > Time.time) return;

        healCancelled = true;

        GetComponent<SpriteRenderer>().material.shader = Shader.Find("GUI/Text Shader");
        GetComponent<SpriteRenderer>().color = Color.white;

        State = PlayerState.Hit;

        damagedTime = Time.time + 0.8f;
        transitioner.transition.SetTrigger("Hurt");
        if (collider.transform.position.x < transform.position.x)
            rb.velocity = new Vector2(DamagePush, DamagePush / 3);
        else
            rb.velocity = new Vector2(-DamagePush, DamagePush / 3);

        GetComponentInChildren<CameraManager>().TriggerShake(1.5f, 10f);
        for (int i = 0; i < dmg; i++)
        {
            if (health > 0)
            {
                health -= 1;
                healthBar.GetComponentsInChildren<SpriteRenderer>()[health].enabled = false;
            }
        }
        if (health <= 0)
            StartCoroutine(Die());
    }

    private void UpdateHitState()
    {
        if (damagedTime -0.65 < Time.time)
        {
            GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (damagedTime - 0.5 < Time.time)
        {
            EnterMovementState();
        }
    }

    private IEnumerator EnterAttackState(int n)
    {
        walled = false;
        if (dashingTime -0.5f > Time.time)
            yield return new WaitForSeconds(dashingTime - Time.time);

        if (!healing && (attackingTime < Time.time && State != PlayerState.Attack) && ((n == 3) || (n == 2 && energyLevel > 2)))
        {
            State = PlayerState.Attack;
            attackStyle = n;

            if (attackStyle == 2)
            {
                StartCoroutine(ChangeEnergy(-3));
                attackingDirection = 0;
                attackVFX[0].SetActive(true);
                foreach (ParticleSystem PS in Trails)
                    PS.emissionRate = 50;
                GetComponent<SpriteRenderer>().enabled = false;
                bounceEffect = Vector2.zero;
                gameObject.GetComponent<BoxCollider2D>().enabled = false;
                groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                if (floating)
                {
                    floating = false;
                    rb.gravityScale = 10;
                }
            }

            else if (attackStyle == 3)
            {
                if (direction.y > 0)
                    attackingDirection = 2;
                else if (direction.y < 0 && !grounded)
                    attackingDirection = 0;
                else
                    attackingDirection = 1;

                attackVFX[1].SetActive(true);
                if (!facingRight)
                {
                    attackVFX[1].transform.rotation = Quaternion.Euler(0, 0, (attackingDirection - 2) * -90 - 35);
                    attackVFX[1].transform.position = new Vector2(transform.position.x - attackPos[1, attackingDirection].x, transform.position.y + attackPos[1, attackingDirection].y);
                }
                else if (facingRight)
                {
                    attackVFX[1].transform.rotation = Quaternion.Euler(0, 0, (attackingDirection - 2) * 90 + 35);
                    attackVFX[1].transform.position = (Vector2)transform.position + attackPos[1, attackingDirection];
                }

                attackingTime = 0.25f + Time.time;
            }
        }
        yield return null;
    }

    private void UpdateAttackState()
    {
        if (attackingTime - 0.4 > Time.time && attackStyle == 3) //last second control
            Flip();

        attackPoint = (Vector2)transform.position + attackPos[attackStyle - 2, attackingDirection]; // need to get rid of all of these and just simplify them
                                                                                                    // v these ones too
        if (!facingRight)
            attackPoint = new Vector2(transform.position.x - attackPos[attackStyle - 2, attackingDirection].x, transform.position.y + attackPos[attackStyle - 2, attackingDirection].y);

        // hitting enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRadii[attackStyle-2], Enemy);

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
        // finished hitting enemies
        
        Jump();

        if ((attackingTime < Time.time && attackStyle != 2) || (grounded && attackingDirection == 0)) // exit 
        {
            foreach (GameObject VFX in attackVFX) // reset VFX
            {
                VFX.SetActive(false);
            }

            jumping = false;
            falling = false;

            Flip();
            EnterMovementState();

            if (attackStyle == 2)
            {
                jumped = false;
                bounceEffect = Vector2.zero;

                if (impactPrefab != null)
                {
                    var impactVFX = Instantiate(impactPrefab, transform) as GameObject;
                    Destroy(impactVFX, 1.5f);
                }
                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;

                damagedTime = Time.time + 1f;

                foreach (ParticleSystem PS in Trails)
                    PS.emissionRate = 0;
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        // managing movement from attack
        bounceEffect.x *= 0.5f;
        bounceEffect.y *= 0.95f;
        if (attackStyle == 3)
            rb.velocity = new Vector2(direction.x * speed * 0.3f, rb.velocity.y) + bounceEffect;
        else
            rb.velocity = new Vector2(0, -40);
    }

    private void AttackBounce(int orri)
    {
        switch (orri)
        {
            case 0:
                Debug.Log("Down");
                rb.velocity = new Vector2(rb.velocity.x, 40);
                jumping = true;
                break;
            case 1:
                Debug.Log("Right");
                bounceEffect = new Vector2(-attackBounce, 0);
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

        if (healing && healingTime < Time.time) //heal
        {
            if (!healCancelled && health < 5)
            {
                healing = false;
                stoppedHealing = true;
                healCancelled = true;
                healthBar.GetComponentsInChildren<SpriteRenderer>()[health].enabled = true;
                health += 1;
                StartCoroutine(ChangeEnergy(-3));
            }
        }

        if (bounceEffect.x > 0.05 || bounceEffect.x < -0.05)
            bounceEffect.x *= 0.5f;
        if (bounceEffect.y > 0.05f)
            bounceEffect.y *= 0.6f;
        if (lSpeedMult < 1)
            lSpeedMult += 0.05f;
        if (rSpeedMult < 1)
            rSpeedMult += 0.05f;

        //walk
        if (dashingTime - 0.5f < Time.time && damagedTime - 0.5 < Time.time && !healing)
        {
            if (direction.x < 0)
                rb.velocity = new Vector2(direction.x * lSpeedMult * speed, rb.velocity.y) + bounceEffect;
            else
                rb.velocity = new Vector2(direction.x * rSpeedMult * speed, rb.velocity.y) + bounceEffect;
        }

        walled = IsWalled();

        if (!floating)
            Jump();

    }

    private void Jump()
    {
        //jump
        if (!jumped && jumping && grounded && !healing)
        {
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 40);
        }
        else if (walled && !grounded && !jumped && jumping)
        {
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 30);
            bounceEffect.x = direction.x * -140;
            if (direction.x > 0)
                rSpeedMult = 0.5f;
            else
                lSpeedMult = 0.5f;
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
        {
            falling = true;
            jumping = false;
        }

        if (floating)
            rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    private void Interact()
    {

    }

    public bool IsGrounded() { return Physics2D.OverlapBox(groundCheck.position, new Vector2(width, 0.1f), 0, groundLayer); }
    public bool IsWalled() 
    {
        if (Physics2D.OverlapCircle((Vector2)transform.position + wallPos1, wallRadius, groundLayer) && direction.x < 0)
            return true;
        else if (Physics2D.OverlapCircle((Vector2)transform.position + wallPos2, wallRadius, groundLayer) && direction.x > 0)
            return true;
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

    public IEnumerator Die()
    {
        State = PlayerState.Death;
        rb.gravityScale = 0;
        deathFX.GetComponent<VisualEffect>().Play();
        deathFX.GetComponent<ParticleSystem>().Play();
        rb.velocity = Vector2.zero;
        GetComponent<SpriteRenderer>().enabled = false;
        deathAnim.Play("DeathAnim");
        yield return new WaitForSeconds(0.9f);
        GetComponent<SpriteRenderer>().enabled = true;
        State = PlayerState.Movement;
        rb.gravityScale = 10;
    }

    public IEnumerator ChangeEnergy(int changeN)
    {
        if ((energyLevel + changeN <= 8) && (energyLevel + changeN >= 0))
        {
            for (int i = 0; i < Math.Abs(changeN); i++)
            {
                yield return new WaitForSeconds(0.12f);
                energyLevel += changeN / Math.Abs(changeN);
                energyOrb.sprite = orbs[energyLevel];
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere((Vector2)transform.position + wallPos2, wallRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + wallPos1, wallRadius);
        //Gizmos.DrawWireSphere(attackPoint, attackRadii[attackStyle - 2]);
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(width, 0.1f, 1));
    }
}
