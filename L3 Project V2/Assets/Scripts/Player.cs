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
    //personal references
    [SerializeField] private GameObject deathFX;
    [SerializeField] private GameObject slamImpactFX;
    [SerializeField] private GameObject[] attackFX = new GameObject[2];
    [SerializeField] private Sprite[] orbs = new Sprite[9]; //for animating orb
    [SerializeField] private List<ParticleSystem> Trails = new List<ParticleSystem>();
    [SerializeField] private Transform groundCheck; // used for checking whether connected to ground
    [SerializeField] private TrailRenderer tr; //used for dash trail
    [SerializeField] private LayerMask groundLayer; //the layermasks are used to check collisions
    [SerializeField] private LayerMask enemy;
    [SerializeField] private LayerMask transition;
    [SerializeField] private List<Sprite> spriteList = new List<Sprite>(); //used in dialogue
    [SerializeField] private List<string> nameList = new List<string>(); //parallel with spriteList
    public Rigidbody2D rb;

    //local scene references
    private SpriteRenderer[] healthBar;
    private Animator deathAnim;
    private LevelLoader transitioner;
    private SpriteRenderer energyOrb;
    public Dialogue dialogue; //contains dialogue object

    //the locations for checks
    private readonly float[,] attackPos = new float[3,2] { { 0.21f, -0.98f }, { 1f, -0.1f }, { 0.37f, 0.57f } };
    private const float groundCheckwidth = 1f;
    private const float wallRadius = 0.15f;
    private readonly Vector2 wallPos1 = new Vector2(-0.67f, -0.25f);
    private readonly Vector2 wallPos2 = new Vector2(0.48f, -0.25f);

    //constant variables
    private const float attackBounce = 40; //used twice, constant
    private const float damagePush = 20; //used too much
    private const float dashingPower = 100;
    private const float speed = 18; //used 3-5 times

    //changing variables
    public PlayerState State = PlayerState.Movement; //contains the animation state
    private int health = 5;
    private int energyLevel = 0;
    private int damage = 3; //needs to be changed so that other atks do different dmg
    private int dialogueCounter = 0; //contains the amount of dialogue lines in an interaction
    public Interactable Interactable; //contains interaction point data

    //speed variables
    public Vector2 Direction;
    private float lSpeedMult = 1; //these two alter the speed when attacking and stuff
    private float rSpeedMult = 1;
    private Vector2 bounceEffect;

    //atk variables
    public int AttackStyle = 0; //used a lot for checks
    public int AttackingDirection = 0;
    private Vector3 attackPoint;

    //timings
    private float attackingTime = 0;
    private float damagedTime = 0;
    private float dashingTime = 0f;
    private float textTime = 0;
    public float HealingTime = 0;
    private float floatTimer = 0;

    //bools
    public bool Grounded = false;
    public bool Walled = false;
    public bool DoubleAtk = false; //kinda unsure about this..
    public bool Healing = false; //used a lot for checks
    public bool HealCancelled = false;
    private bool facingRight = true;
    private bool floating = false;
    private bool stoppedHealing = true;
    private bool transitioning = false;
    private bool canDash = false; //used for only one dash in air
    private bool jumping = false;
    private bool falling = false;
    private bool jumped = false;
    private bool interacting = false;
    private bool skipBtnPressed = false;
    private bool switchingDialogue = false;
    private bool interacted = false;
    private bool firstTransition = true;
    private bool startDashWalled = false;
    private bool dashing = false;


    public void SetLocalVariables()
    {
        healthBar = GameObject.Find("HealthBar").GetComponentsInChildren<SpriteRenderer>();
        dialogue = GameObject.Find("HealthBar").GetComponent<Dialogue>();
        deathAnim = GameObject.Find("NewCam2").GetComponent<Animator>();
        transitioner = GameObject.Find("LevelLoader").GetComponent<LevelLoader>();
        energyOrb = GameObject.Find("OrbSheet_0").GetComponent<SpriteRenderer>();
    }

    public void OnMove(InputValue value)
    {
        if (interacting || transitioning) return;
        Direction = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        jumping = value.Get<float>() > 0.5f;
        if (!jumping)
            jumped = false;
    }

    public void OnEscape(InputValue value)
    {
        foreach (Enemy E in GameObject.FindObjectsOfType<Enemy>())
        {
            E.Pause();
        }

        //Player.Pause()

        Debug.Log("Escape");

        StartCoroutine(transitioner.LoadLevel(0)); //exit levels and just go to inventory scene. Needs some custom code desperately
    }

        public void OnStab(InputValue value)
    {
        StartCoroutine(EnterAttackState(3));
    }

    public void OnDash(InputValue value)
    {
        StartCoroutine(EnterDashState());
    }

    public void OnInteract(InputValue value)
    {
        if (Interactable != null)
        {
            if (!interacting)
                StartCoroutine(InteractionPostponer());
            else
                skipBtnPressed = true;
        }
    }

    private IEnumerator InteractionPostponer()
    {
        if (State == PlayerState.Attack)
        {
            if (AttackStyle == 2)
                Grounded = true;
            else
                yield return new WaitForSeconds(attackingTime - Time.time);

            UpdateAttackState();
        }
        GetComponent<ParticleSystem>().emissionRate = 0;
        transform.GetChild(9).GetComponent<ParticleSystem>().emissionRate = 0;
        HealCancelled = true;
        GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
        GetComponent<SpriteRenderer>().color = Color.white;
        if (dashingTime - 0.5f > Time.time)
        {
            SetDash(false, 10, Vector2.zero);
        }
        rb.velocity = Vector3.zero;
        Direction = Vector2.zero;
        interacting = true;

        Interact();
    }

    public void OnFloat(InputValue value)
    {
        if (value.Get<float>() > 0.5f && !(Healing && HealingTime < Time.time) && !interacting && dashingTime - 0.5f < Time.time && (State != PlayerState.Attack || AttackStyle == 3) && energyLevel > 0)
        {
            floating = true;
            rb.gravityScale = 0;
            floatTimer = Time.time + 0.4f;
        }
        else
        {
            floating = false;
            rb.gravityScale = 10;
            floatTimer = 0;
        }
    }

    public void OnSlam(InputValue value)
    {
        StartCoroutine(EnterAttackState(2));
    }

    public void OnHeal(InputValue value)
    {
        Healing = value.Get<float>() > 0.5f &&  State == PlayerState.Movement && Grounded && !interacting;

        if (stoppedHealing && Healing && energyLevel > 2) // Is true after pressing button down
        {
            StartCoroutine(HealPostponer());
        }

        if (!Healing)
            stoppedHealing = true;
        else
            stoppedHealing = false;
    }

    private IEnumerator HealPostponer()
    {
        if (dashingTime - 0.5f > Time.time)
            yield return new WaitForSeconds(dashingTime - 0.5f - Time.time);
        if (attackingTime > Time.time)
            yield return new WaitForSeconds(attackingTime - Time.time);
        if (!Healing || energyLevel < 3 || health == 5) yield break;
        HealingTime = Time.time + 1;
        HealCancelled = false;
        rb.velocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        Grounded = IsGrounded();

        if (transitioning || (Healing && HealingTime > Time.time))
            return;
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

        switch (State)
        {
            case PlayerState.Movement:
                if (!dashing)
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

        if (floating || (Walled && !Grounded && !jumping))
            rb.velocity = new Vector2(rb.velocity.x, 0);
        if (floating && floatTimer < Time.time)
        {
            StartCoroutine(ChangeEnergy(-1));
            floatTimer = Time.time + 0.4f;
            if (energyLevel < 1)
            {
                floating = false;
                rb.gravityScale = 10;
                floatTimer = 0;
                Debug.Log("Working");
            }
        }
    }

    private IEnumerator EnterDashState()
    {
        if (State == PlayerState.Attack && AttackStyle == 3 && attackingTime - 0.15f < Time.time)
            yield return new WaitForSeconds(attackingTime - Time.time);

        if (interacting || !canDash || dashingTime > Time.time || (Healing && HealingTime < Time.time) || health <= 0 || (State == PlayerState.Attack && AttackStyle == 2)) yield break;

        canDash = false;
        if (Walled)
        {
            transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            SetDash(true, 0, new Vector2(transform.localScale.x * dashingPower, 0));
            startDashWalled = true;
        }
        else
            SetDash(true, 0, new Vector2(transform.localScale.x * dashingPower, 0));
        dashingTime = Time.time + 0.6f;
    }

    public void EnterHitState(GameObject collider, int dmg)
    {
        if (damagedTime > Time.time) return;

        HealCancelled = true;

        GetComponent<SpriteRenderer>().material.shader = Shader.Find("GUI/Text Shader");
        GetComponent<SpriteRenderer>().color = Color.white;

         State = PlayerState.Hit;

        damagedTime = Time.time + 0.8f;
        transitioner.Transition.SetTrigger("Hurt");
        if (collider.transform.position.x < transform.position.x)
            rb.velocity = new Vector2(damagePush, damagePush / 3);
        else
            rb.velocity = new Vector2(-damagePush, damagePush / 3);

        GetComponentInChildren<CameraManager>().TriggerShake(1.5f, 10f);
        for (int i = 0; i < dmg; i++)
        {
            if (health > 0)
            {
                health -= 1;
                healthBar[health].enabled = false;
            }
        }
        if (health <= 0)
        {
            if (dashingTime - 0.5f > Time.time)
                SetDash(false, 10, Vector2.zero);
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
            SetDash(false, 10, Vector2.zero);
        }
        if (damagedTime - 0.5 < Time.time)
        {
            EnterMovementState();
        }
    }

    private IEnumerator EnterAttackState(int n)
    {
        Walled = false;

        if (n == 2 && attackingTime < Time.time)
            yield return new WaitForSeconds(attackingTime - Time.time);

        if (!interacting && !(Healing && HealingTime > Time.time) && (attackingTime + 0.1f)  < Time.time && State != PlayerState.Attack && ((energyLevel > 2 && n == 2) || n == 3))
        {
            if (dashingTime - 0.5f > Time.time)
                yield return new WaitForSeconds(dashingTime - 0.5f - Time.time);

            State = PlayerState.Attack;
            AttackStyle = n;

            if (AttackStyle == 2)
                EnterSlam();

            else if (AttackStyle == 3)
                EnterSlash();
        }
        yield return null;
    }

    private void EnterSlam()
    {
        Debug.Log(energyLevel);
        StartCoroutine(ChangeEnergy(-3));
        AttackingDirection = 0;
        attackFX[0].SetActive(true);
        foreach (ParticleSystem PS in Trails)
            PS.emissionRate = 50;
        GetComponent<SpriteRenderer>().enabled = false;
        bounceEffect = Vector2.zero;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        groundCheck.GetComponent<BoxCollider2D>().enabled = true;
        if (floating)
        {
            floating = false;
            rb.gravityScale = 10;
        }
    }
    private void EnterSlash()
    {
        if ((attackingTime - 0.1f) < Time.time && (attackingTime + 0.1f) < Time.time)
            DoubleAtk = true;

        if (Direction.y > 0)
            AttackingDirection = 2;
        else if (Direction.y < 0 && !Grounded)
            AttackingDirection = 0;
        else
            AttackingDirection = 1;

        attackFX[1].SetActive(true);
        if (!facingRight) // VFX
        {
            attackFX[1].transform.rotation = Quaternion.Euler(0, 0, (AttackingDirection - 2) * -90 - 35);
            attackFX[1].transform.position = new Vector2(transform.position.x - attackPos[AttackingDirection, 0], transform.position.y + attackPos[AttackingDirection, 1]);
        }
        else if (facingRight)
        {
            attackFX[1].transform.rotation = Quaternion.Euler(0, 0, (AttackingDirection - 2) * 90 + 35);
            attackFX[1].transform.position = (Vector2)transform.position + new Vector2(attackPos[AttackingDirection, 0], attackPos[AttackingDirection, 1]);
        }

        attackingTime = 0.2f + Time.time;
    }

    private void UpdateAttackState()
    {
        if (attackingTime - 0.2 > Time.time && AttackStyle == 3) //last second control
            Flip();

        attackPoint = (Vector2)transform.position + new Vector2(attackPos[AttackingDirection, 0], attackPos[AttackingDirection, 1]);
        if (!facingRight)
            attackPoint = new Vector2(transform.position.x - attackPos[AttackingDirection, 0], transform.position.y + attackPos[AttackingDirection, 1]);

        // hitting enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, 1.55f, enemy);

        if (hitEnemies.Length != 0)
            GetComponentInChildren<CameraManager>().TriggerShake(1f, 15f); // camera shake

        foreach (Collider2D enemy in hitEnemies)
        {
            if (!facingRight && AttackingDirection == 1)
            {
                StartCoroutine(enemy.GetComponent<Enemy>().Hit(damage, 3));
                AttackBounce(3);
            }
            else
            {
                StartCoroutine(enemy.GetComponent<Enemy>().Hit(damage, AttackingDirection));
                AttackBounce(AttackingDirection);
            }
            if (AttackingDirection == 1)
            {
                canDash = true;
            }
        }
        // finished hitting enemies
        
        StartCoroutine(Jump());

        if ((attackingTime < Time.time && AttackStyle != 2) || (Grounded && AttackingDirection == 0)) // exit 
        {
            foreach (GameObject VFX in attackFX) // reset VFX
            {
                VFX.SetActive(false);
            }

            jumping = false;
            falling = false;

            Flip();
            EnterMovementState();

            if (AttackStyle == 2) //slamming
            {
                jumped = false;
                bounceEffect = Vector2.zero;

                var impactVFX = Instantiate(slamImpactFX, transform) as GameObject;
                Destroy(impactVFX, 1.5f);

                gameObject.GetComponent<BoxCollider2D>().enabled = true;
                groundCheck.GetComponent<BoxCollider2D>().enabled = false;

                damagedTime = Time.time + 1f; // invcibility frames

                foreach (ParticleSystem PS in Trails)
                    PS.emissionRate = 0;
                GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        // managing movement from attack
        bounceEffect.x *= 0.5f;
        bounceEffect.y *= 0.95f;
        if (AttackStyle == 3 && (dashingTime - 0.5f) < Time.time)
        {
            if (AttackingDirection != 0 && AttackingDirection != 2)
                rb.velocity = new Vector2(Direction.x * speed * 0.3f, rb.velocity.y) + bounceEffect;
            else
                rb.velocity = new Vector2(Direction.x * speed, rb.velocity.y) + bounceEffect;
        }
        else if (AttackStyle == 2)
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

        if (Grounded)
            canDash = true;

        if (dashingTime - 0.5f < Time.time && dashing)
            SetDash(false, 10, rb.velocity);

        if (Healing && HealingTime < Time.time) //heal
        {
            if (!HealCancelled && health < 5)
            {
                Healing = false;
                stoppedHealing = true;
                HealCancelled = true;
                healthBar[health].enabled = true;
                health += 1;
                StartCoroutine(ChangeEnergy(-3));
            }
        }

        if (bounceEffect.x > 0.05 || bounceEffect.x < -0.05)
            bounceEffect.x *= 0.5f;
        else
            bounceEffect.x = 0;
        if (bounceEffect.y > 0.05f)
            bounceEffect.y *= 0.6f;
        else
            bounceEffect.y = 0;
        if (lSpeedMult < 1)
            lSpeedMult += 0.05f;
        if (rSpeedMult < 1)
            rSpeedMult += 0.05f;

        //walk
        if (damagedTime - 0.5 < Time.time && !(Healing && HealingTime > Time.time))
        {
            if (Direction.x < 0)
                rb.velocity = new Vector2(Direction.x * lSpeedMult * speed, rb.velocity.y) + bounceEffect;
            else
                rb.velocity = new Vector2(Direction.x * rSpeedMult * speed, rb.velocity.y) + bounceEffect;
            if (Grounded && Direction.x != 0)
                GetComponent<ParticleSystem>().emissionRate = 15;
            else
                GetComponent<ParticleSystem>().emissionRate = 0;
        }
        else
            GetComponent<ParticleSystem>().emissionRate = 0;
        if (dashingTime - 0.5f > Time.time)
            rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0);

        Walled = IsWalled() && !(Healing && HealingTime > Time.time) && !floating && State != PlayerState.Attack && !interacting;
        if (Walled && dashingTime - 0.5f > Time.time && !startDashWalled)
            SetDash(false, 10, Vector2.zero);
        if (Walled)
            transform.GetChild(9).GetComponent<ParticleSystem>().emissionRate = 10;
        else
            transform.GetChild(9).GetComponent<ParticleSystem>().emissionRate = 0;
        if (!floating)
            StartCoroutine(Jump());
    }

    private IEnumerator Jump()
    {
        //jump
        if (!jumped && jumping && Grounded && !(Healing && HealingTime < Time.time))
        {
            if (dashingTime - 0.5f > Time.time)
                yield return new WaitForSeconds(dashingTime - 0.5f - Time.time);
            
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 45);
        }
        else if (Walled && !Grounded && !jumped && jumping)
        {
            jumped = true;
            rb.velocity = new Vector2(rb.velocity.x, 35);
            bounceEffect.x = Direction.x * -140;
            if (Direction.x > 0)
                rSpeedMult = 0.5f;
            else
                lSpeedMult = 0.5f;
        }
        else if (Grounded && falling)
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
        string[] chats = Interactable.Dialogue[Interactable.DialogueNums].Split(" | ");
        string chat = "";
        if (dialogueCounter < chats.Length)
        {
            chat = chats[dialogueCounter].Split(" / ")[1];
        }

        if (!interacted)
        {
            //move down the dialogue box, set up picture + name, make it the right orrientation
            StartCoroutine(dialogue.MoveDialogue('D'));
            interacted = true;
            int x = Int32.Parse(chats[dialogueCounter].Split(" / ")[0]);
            dialogue.ResetSide();
            dialogue.FirstNameAndPicture(spriteList[x], nameList[x]);
            foreach (Enemy E in GameObject.FindObjectsOfType<Enemy>())
            {
                E.Pause();
            }
        }

        if (skipBtnPressed)
        {
            if (!switchingDialogue) //if text hasn't finished displaying
            {
                dialogue.Text.text = chat; // prepare for switching and fully display text
                switchingDialogue = true;
            }
            else //if a transition is required
            {
                if (dialogueCounter < chats.Length - 1) //dialogue remains for display
                {
                    dialogueCounter++;
                    int x = Int32.Parse(chats[dialogueCounter].Split(" / ")[0]);
                    StartCoroutine(dialogue.SwitchDialogue(spriteList[x], nameList[x], this)); // SwitchDialogue(sprite, text, player);
                    dialogue.Text.text = "";
                }
                else //interaction finished
                {
                    StartCoroutine(dialogue.MoveDialogue('U'));
                    dialogue.Text.text = "";
                    interacting = false;
                    interacted = false;
                    dialogueCounter = 0;
                    foreach (Enemy E in GameObject.FindObjectsOfType<Enemy>())
                    {
                        StartCoroutine(E.UnPause());
                    }
                    if (Interactable.Dialogue.Count-1 > Interactable.DialogueNums)
                        Interactable.DialogueNums++;
                    foreach (char item in Interactable.ImpactfulNums.ToCharArray())
                    {
                        if (Interactable.DialogueNums == item)
                            Interactable.DialogImpact();
                    }
                }
                switchingDialogue = false;
            }
            skipBtnPressed = false;
            return;
        }

        if (dialogue.Text.text.Length < chat.Length && textTime < Time.time && dialogue.gameObject.GetComponent<Animator>().speed == 0) // if characters remain, all animations have completed, and wait has been completed
        {
            dialogue.Text.text += chat.ToCharArray()[dialogue.Text.text.Length];
            textTime = Time.time + 0.04f;
        }
        else if (dialogue.Text.text.Length == chat.Length)
            switchingDialogue = true;
    }

    public void StopSwitchingDialogue()
    {
        switchingDialogue = false;
    }

    private void SetDash(bool trueFalse, int gravity, Vector2 dashSpeed)
    {
        dashing = trueFalse;
        tr.emitting = trueFalse;
        rb.gravityScale = gravity;
        gameObject.GetComponent<BoxCollider2D>().enabled = !trueFalse;
        groundCheck.GetComponent<BoxCollider2D>().enabled = trueFalse;
        rb.velocity = dashSpeed;
        if (startDashWalled && !trueFalse)
        {
            transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            startDashWalled = false;
        }
    }

    public bool IsGrounded() { return Physics2D.OverlapBox(groundCheck.position, new Vector2(groundCheckwidth, 0.1f), 0, groundLayer); }
    public bool IsWalled() 
    {
        if (Physics2D.OverlapCircle((Vector2)transform.position + wallPos1, wallRadius, LayerMask.GetMask("Walls")) && Direction.x < 0)
            return true;
        else if (Physics2D.OverlapCircle((Vector2)transform.position + wallPos2, wallRadius, LayerMask.GetMask("Walls")) && Direction.x > 0)
            return true;
        else
            return false;
    }

    private void Flip()
    {
        if (facingRight && Direction.x < 0 || !facingRight && Direction.x > 0)
        {
            facingRight = !facingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & transition) != 0 && transitioner != null) // if it's a transition collision
        {
            if (!firstTransition)
            {
                StartCoroutine(WalkAnim(collision.gameObject.GetComponent<Transitioner>(), 1));
                // prepare any possible cutscenes
                // save player data and input it into next scene
                // output player into the correct location, and make them walk
                // (make the corrosponding transitioners have the same name, so you can check for the right one, and output them correctly).
                GM.Instance.transitionID = collision.gameObject.GetComponent<Transitioner>().id;
                StartCoroutine(transitioner.LoadLevel(collision.gameObject.GetComponent<Transitioner>().nextScene));
            }
            else
                StartCoroutine(WalkAnim(collision.gameObject.GetComponent<Transitioner>(), -1));
        }
    }

    private IEnumerator WalkAnim(Transitioner t, int multiplier)
    {
        bool fT = firstTransition;
        if (fT)
            yield return new WaitForSeconds(0.4f);
        transitioning = true;
        Direction = multiplier * t.direction;
        transform.localScale = new Vector2(t.direction.x * multiplier, 1);
        if (facingRight && t.direction.x * multiplier < 0)
            facingRight = false;
        if (Direction.x <= 0) // make 'em walk
            rb.velocity = new Vector2(Direction.x * lSpeedMult * speed, rb.velocity.y) + bounceEffect;
        else
            rb.velocity = new Vector2(Direction.x * rSpeedMult * speed, rb.velocity.y) + bounceEffect;
        if (fT)
            yield return new WaitForSeconds(0.2f);
        else
            yield return new WaitForSeconds(2f);
        transitioning = false;
        Direction = Vector2.zero;
        rb.velocity = Vector2.zero;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & transition) != 0 && transitioner != null) // if it's a transition collision
            if (firstTransition)
                firstTransition = false;
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
            int counterN = energyLevel;
            energyLevel += changeN;
            for (int i = 0; i < Math.Abs(changeN); i++)
            {
                yield return new WaitForSeconds(0.12f);
                counterN += changeN / Math.Abs(changeN);
                energyOrb.sprite = orbs[counterN];
            }
        }
        else if (floating)
            floating = false;
    }

    public void SetStates(int Health, int EnergyLevel)
    {
        int dmg = 5 - Health;
        for (int i = 0; i < dmg; i++)
        {
            health -= 1;
            healthBar[health].enabled = false;
        }
        energyLevel = EnergyLevel;
        energyOrb.sprite = orbs[energyLevel];
        //which skills are allowed at this point
    }

    private void OnDisable()
    {
        //store state
        GM.Instance.health = health;
        GM.Instance.energyLevel = energyLevel;
    }
}
