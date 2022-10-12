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
    public Vector2[] attackPos = new Vector2[3] { new Vector2(1.21f, -0.98f), new Vector2(10f, -0.1f), new Vector2(0.37f, 0.57f) };
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
    public bool doubleAtk = false;
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
    private bool jumped = false;
    public bool facingRight = true;
    public bool grounded = false;
    public bool walled = false;
    public bool floating = false;

    public Interactable interactable;
    private bool interacting = false;
    public Dialogue dialogue;
    private bool skipBtnPressed = false;
    private bool switchingDialogue = false;
    private float textTime = 0;
    private bool interacted = false;
    private int dialogueCounter = 0;
    public List<Sprite> spriteList = new List<Sprite>();
    public List<string> nameList = new List<string>();

    public void OnMove(InputValue value)
    {
        if (interacting) return;
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
        {
            if (!interacting)
            {
                GetComponent<ParticleSystem>().emissionRate = 0;
                transform.GetChild(9).GetComponent<ParticleSystem>().emissionRate = 0;
                healCancelled = true;
                if (dashingTime - 0.5f > Time.time)
                {
                    tr.emitting = false;
                    rb.gravityScale = 10;
                    gameObject.GetComponent<BoxCollider2D>().enabled = true;
                    groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
                rb.velocity = Vector3.zero;
                interacting = true;

                Interact();
            }
            else
            {
                skipBtnPressed = true;
            }
        }
    }

    public void OnFloat(InputValue value)
    {
        if (value.Get<float>() > 0.5f && attackingTime < Time.time && !interacting)
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
        if (interacting)
        {
            Interact();
            return;
        }
        if (damagedTime - 0.65 < Time.time) // if hurt and attacking
        {
            GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
            GetComponent<SpriteRenderer>().color = Color.white;
        }

        grounded = IsGrounded();

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
        if (interacting || !canDash || dashingTime > Time.time || healing || health <= 0) return;

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
        {
            if (dashingTime - 0.5f > Time.time)
            {
                tr.emitting = false;
                rb.gravityScale = 10;
                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                rb.velocity = new Vector2(0, 0);
            }
            StartCoroutine(Die());
        }
    }

    private void UpdateHitState()
    {
        if (damagedTime -0.65 < Time.time)
        {
            GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (dashingTime - 0.5f < Time.time)
        {
            tr.emitting = false;
            rb.gravityScale = 10;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            groundCheck.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            rb.velocity = new Vector2(0, 0);
        }
        if (damagedTime - 0.5 < Time.time)
        {
            EnterMovementState();
        }
    }

    private IEnumerator EnterAttackState(int n)
    {
        walled = false;

        if (!interacting &&!healing && (attackingTime - 0.1f)  < Time.time && State != PlayerState.Attack)
        {
            if (dashingTime - 0.5f > Time.time)
                yield return new WaitForSeconds(dashingTime - 0.5f - Time.time);

            State = PlayerState.Attack;
            attackStyle = n;

            if (attackStyle == 2)
                EnterSlam();

            else if (attackStyle == 3)
                EnterSlash();
        }
        yield return null;
    }

    private void EnterSlam()
    {
        if (energyLevel > 2)
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
    }
    private void EnterSlash()
    {
        if ((attackingTime - 0.1f) < Time.time && (attackingTime + 0.1f) < Time.time)
            doubleAtk = true;

        if (direction.y > 0)
            attackingDirection = 2;
        else if (direction.y < 0 && !grounded)
            attackingDirection = 0;
        else
            attackingDirection = 1;

        attackVFX[1].SetActive(true);
        if (!facingRight) // VFX
        {
            attackVFX[1].transform.rotation = Quaternion.Euler(0, 0, (attackingDirection - 2) * -90 - 35);
            attackVFX[1].transform.position = new Vector2(transform.position.x - attackPos[attackingDirection].x, transform.position.y + attackPos[attackingDirection].y);
        }
        else if (facingRight)
        {
            attackVFX[1].transform.rotation = Quaternion.Euler(0, 0, (attackingDirection - 2) * 90 + 35);
            attackVFX[1].transform.position = (Vector2)transform.position + attackPos[attackingDirection];
        }

        attackingTime = 0.25f + Time.time;
    }

    private void UpdateAttackState()
    {
        if (attackingTime - 0.2 > Time.time && attackStyle == 3) //last second control
            Flip();

        attackPoint = (Vector2)transform.position + attackPos[attackingDirection];
        if (!facingRight)
            attackPoint = new Vector2(transform.position.x - attackPos[attackingDirection].x, transform.position.y + attackPos[attackingDirection].y);

        // hitting enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, 1.55f, Enemy);

        if (hitEnemies.Length != 0)
            GetComponentInChildren<CameraManager>().TriggerShake(1f, 15f); // camera shake

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
            if (attackingDirection == 1)
            {
                canDash = true;
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

            if (attackStyle == 2) //slamming
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

                damagedTime = Time.time + 1f; // invcibility frames

                foreach (ParticleSystem PS in Trails)
                    PS.emissionRate = 0;
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        // managing movement from attack
        bounceEffect.x *= 0.5f;
        bounceEffect.y *= 0.95f;
        if (attackStyle == 3 && (dashingTime - 0.5f) < Time.time)
            rb.velocity = new Vector2(direction.x * speed * 0.3f, rb.velocity.y) + bounceEffect;
        else if (attackStyle == 2)
            rb.velocity = new Vector2(0, -40);
    }

    private void AttackBounce(int orri)
    {
        switch (orri)
        {
            case 0: // Atking down
                rb.velocity = new Vector2(rb.velocity.x, 40);
                jumping = true;
                break;
            case 1: // Atking right
                bounceEffect = new Vector2(-attackBounce, 0);
                break;
            case 3: // Atking left
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
        if (damagedTime - 0.5 < Time.time && !healing)
        {
            if (direction.x < 0)
                rb.velocity = new Vector2(direction.x * lSpeedMult * speed, rb.velocity.y) + bounceEffect;
            else
                rb.velocity = new Vector2(direction.x * rSpeedMult * speed, rb.velocity.y) + bounceEffect;
            if (grounded && direction.x != 0)
                GetComponent<ParticleSystem>().emissionRate = 15;
            else
                GetComponent<ParticleSystem>().emissionRate = 0;
        }
        else
            GetComponent<ParticleSystem>().emissionRate = 0;
        if (dashingTime - 0.5f > Time.time)
            rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);

            walled = IsWalled();
        if (walled)
            transform.GetChild(9).GetComponent<ParticleSystem>().emissionRate = 10;
        else
            transform.GetChild(9).GetComponent<ParticleSystem>().emissionRate = 0;
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
        string chat = "";
        int num = 0;
        if (dialogueCounter < interactable.Dialogue.Count)
        {
            chat = interactable.Dialogue[dialogueCounter].Split(" / ")[1];
            num = Int32.Parse(interactable.Dialogue[dialogueCounter].Split(" / ")[0]);
            Debug.Log(num);
        }

        if (!interacted)
        {
            //move down the dialogue box, set up picture + name, make it the right orrientation
            StartCoroutine(dialogue.MoveDialogue('D'));
            interacted = true;
            dialogue.FirstNameAndPicture(spriteList[num], nameList[num]);
            foreach (Enemy E in GameObject.FindObjectsOfType<Enemy>())
            {
                E.Pause();
            }
        }

        if (skipBtnPressed)
        {
            if (!switchingDialogue) //if text hasn't finished displaying
            {
                dialogue.text.text = chat;
                switchingDialogue = true;
            }
            else //if a transition is required
            {
                if (dialogueCounter + 1 < interactable.Dialogue.Count) //dialogue remains for display
                {
                    dialogueCounter++;
                    StartCoroutine(dialogue.SwitchDialogue(spriteList[Int32.Parse(interactable.Dialogue[dialogueCounter].Split(" / ")[0])], nameList[Int32.Parse(interactable.Dialogue[dialogueCounter].Split(" / ")[0])], this));
                    dialogue.text.text = "";
                }
                else //interaction finished
                {
                    StartCoroutine(dialogue.MoveDialogue('U'));
                    dialogue.text.text = "";
                    interacting = false;
                    interacted = false;
                    dialogueCounter = 0;
                    foreach (Enemy E in GameObject.FindObjectsOfType<Enemy>())
                    {
                        StartCoroutine(E.UnPause());
                    }
                }
                switchingDialogue = false;
            }
            skipBtnPressed = false;
            return;
        }

        if (dialogue.text.text.Length < chat.Length && textTime < Time.time && dialogue.gameObject.GetComponent<Animator>().speed == 0) // if characters remain, all animations have completed, and wait has been completed
        {
            dialogue.text.text += chat.ToCharArray()[dialogue.text.text.Length];
            textTime = Time.time + 0.04f;
        }
        else if (dialogue.text.text.Length == chat.Length)
            switchingDialogue = true;
    }

    public void StopSwitchingDialogue()
    {
        switchingDialogue = false;
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
        Gizmos.DrawWireSphere((Vector2)transform.position + attackPos[attackingDirection], 1.55f);
        Gizmos.DrawWireCube(groundCheck.position, new Vector3(width, 0.1f, 1));
    }
}
