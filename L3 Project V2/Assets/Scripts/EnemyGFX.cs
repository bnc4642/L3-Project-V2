using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyGFX : MonoBehaviour
{
    public AIPath aiPath;
    public Animator anim;
    public float attackTime = 0.4f;
    public LayerMask player;
    public LayerMask playerWeapon;
    public int dmg;
    public float invicibilityFrames;

    private int health = 6;
    private float attackingTime;
    private int currentState = 0;
    private float lockedTill = 0;
    private float timeHit;

    private static readonly int Attack = Animator.StringToHash("MosqAttack");
    private static readonly int Fly = Animator.StringToHash("MosqFollow");

    void Update()
    {
        int state = 0;
        if (Time.time < lockedTill)
            return;
        if (Physics2D.OverlapCircle(transform.position, 3.2f, player) && Time.time > attackingTime)
        {
            aiPath.maxSpeed = 25;
            attackingTime = Time.time + attackTime + 1.5f;
            lockedTill = Time.time + attackTime;
            state = Attack;
        }
        else
        {
            aiPath.maxSpeed = 3;
            state = Fly;
        }
        if (aiPath.destination.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (state == currentState) { }
        else
        {
            anim.CrossFade(state, 0, 0);
            currentState = state;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & player) != 0)
            collision.gameObject.GetComponent<Player>().Hit(this.gameObject, dmg);
    }

    public void Hit(int dmg)
    {
        if (Time.time > timeHit + invicibilityFrames)
        {
            aiPath.target.GetComponentInChildren<ScreenShake>().TriggerShake(0.5f, 5f);
            health -= dmg;
            if (health <= 0)
                StartCoroutine(Die());
            timeHit = Time.time;
            Debug.Log(health);
        }
    }

    private IEnumerator Die()
    {
        anim.CrossFade(Animator.StringToHash("MosqDeath"), 0, 0);
        Debug.Log("working");
        yield return new WaitForSeconds(0.3f);
        Debug.Log("working");
        foreach (ParticleSystem effect in GetComponentsInChildren<ParticleSystem>())
        {
            effect.Play();
        }

        GetComponent<SpriteRenderer>().enabled = false;
        
        GetComponent<Collider2D>().enabled = false;
        GetComponentInParent<AIPath>().enabled = false;

        this.enabled = false;
    }
}
