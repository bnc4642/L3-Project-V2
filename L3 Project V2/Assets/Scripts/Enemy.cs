using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{

    public AIPath aiPath;

    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Follow = Animator.StringToHash("Follow");
    private static readonly int Idle = Animator.StringToHash("Idle");

    public Animator anim;

    public int health = 20;

    private int currentState = 0;
    private float lockedTill = 0;
    private float timeHit;
    private float attackingTime = 0;
    private float timeDisabled = 0.1f;
    public float invinciFrames = 0.2f;
    public LayerMask player;
    public LayerMask playerWeapon;

    public bool startled = false;
    public bool hitting = false;
    public float patrollingRange = 25;
    public float timeForAttack = 0.4f;
    public float attackingBreak = 1.5f;
    public float attackingRange = 5;
    public float damagePush = 10;
    public int dmg = 3;
    private bool paused = false;

    public GameObject Food;

    public virtual void Init()
    {
        anim = GetComponent<Animator>();
        aiPath.target = GM.Instance.Player.transform;
    }

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
            return;
        int state = 0; 
        if (Time.time < lockedTill)
            return;
        if (Physics2D.OverlapCircle(transform.position, attackingRange, player) && Time.time > attackingTime)
        {
            state = ToAttack();
        }
        else
        {
            state = NotAttacking();
        }

        if (state != currentState)
        {
            anim.CrossFade(state, 0, 0);
            currentState = state;
        }
        if (hitting)
            GM.Instance.Player.GetComponent<Player>().EnterHitState(this.gameObject, dmg);
    }

    public virtual int ToAttack()
    {
        aiPath.maxSpeed = 25;
        attackingTime = Time.time + timeForAttack + attackingBreak;
        lockedTill = Time.time + timeForAttack;
        return Attack;
    }

    public virtual int NotAttacking()
    {
        if (startled || Physics2D.OverlapCircle(transform.position, patrollingRange, player))
        {
            if (aiPath.destination.x < transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }

            startled = true;
            aiPath.maxSpeed = 5;
            return Follow;
        }
        else
        {
            aiPath.maxSpeed = 0;
            return Idle;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & player) != 0)
            hitting = true;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & player) != 0)
            hitting = false;
    }

    public virtual IEnumerator Hit(int dmg, int orrientation)
    {
        if (Time.time > timeHit + invinciFrames)
        {
            StartCoroutine(aiPath.target.GetComponentInChildren<Player>().ChangeEnergy(1));
            aiPath.target.GetComponentInChildren<CameraManager>().TriggerShake(0.5f, 5f);
            health -= dmg;
            if (health <= 0)
                StartCoroutine(Die());
            timeHit = Time.time;
            aiPath.enabled = false;
            HitEffects(orrientation);
            GetComponent<SpriteRenderer>().material.shader = Shader.Find("GUI/Text Shader");

            yield return new WaitForSeconds(timeDisabled);

            GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");

            yield return new WaitForSeconds(timeDisabled);

            aiPath.enabled = true;
        }
    }

    public virtual void HitEffects(int orri)
    {
        GetComponentsInChildren<ParticleSystem>()[3].Play();
        switch (orri)
        {
            case 0: // down
                GetComponentInParent<Rigidbody2D>().velocity = new Vector2(0, -damagePush / 3);
                GetComponentsInChildren<ParticleSystem>()[3].gameObject.transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case 1: // right
                GetComponentInParent<Rigidbody2D>().velocity = new Vector2(damagePush, damagePush / 3);
                GetComponentsInChildren<ParticleSystem>()[3].gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 2: // up
                GetComponentInParent<Rigidbody2D>().velocity = new Vector2(0, damagePush / 3);
                GetComponentsInChildren<ParticleSystem>()[3].gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 3: // left
                GetComponentInParent<Rigidbody2D>().velocity = new Vector2(-damagePush, damagePush / 3);
                GetComponentsInChildren<ParticleSystem>()[3].gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            default:
                break;
        }
    }

    public virtual IEnumerator Die()
    {
        anim.CrossFade(Animator.StringToHash("Death"), 0, 0);
        yield return new WaitForSeconds(0.3f);
        GetComponentsInChildren<ParticleSystem>()[3].enableEmission = false;
        GetComponentsInChildren<ParticleSystem>()[0].Play();
        GetComponentsInChildren<ParticleSystem>()[1].Play();
        GetComponentsInChildren<ParticleSystem>()[2].Play();

        GetComponent<SpriteRenderer>().enabled = false;
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            GameObject f = Instantiate(Food);
            f.transform.position = transform.position;
        }

        GetComponent<Collider2D>().enabled = false;
        GetComponentInParent<AIPath>().enabled = false;
        GetComponentInParent<Rigidbody2D>().velocity = Vector3.zero;
        yield return new WaitForSeconds(0.5f);
        Destroy(transform.parent.gameObject);
    }

    public void Pause() // During interaction
    {
        GetComponentInParent<AIPath>().enabled = false;
        GetComponentInParent<Rigidbody2D>().velocity = Vector3.zero;
        paused = true;
        anim.CrossFade(Animator.StringToHash("Idle"), 0, 0);
    }

    public void UnPause() // After interaction
    {
        GetComponentInParent<AIPath>().enabled = true;
        paused = false;
    }
}
