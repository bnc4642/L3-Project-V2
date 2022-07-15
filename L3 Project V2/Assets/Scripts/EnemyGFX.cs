using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyGFX : MonoBehaviour
{
    public AIPath aiPath;
    public Animator anim;
    public float attackTime;
    public float radius;
    public LayerMask player;
    public LayerMask playerWeapon;
    public int dmg;
    public float invicibilityFrames;

    private int health = 6;
    private float attackingTime;
    private int currentState = 0;
    private float lockedTill = 0;
    private bool invincibilityFraming;

    private static readonly int Attack = Animator.StringToHash("MosqAttack");
    private static readonly int Fly = Animator.StringToHash("MosqFollow");

    void Update()
    {
        int state = 0;
        if (Time.time < lockedTill)
            return;
        if (Physics2D.OverlapCircle(transform.position, radius, player) && Time.time > attackingTime)
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

    public IEnumerator Hit(int dmg)
    {
        if (!invincibilityFraming)
        {
            aiPath.target.GetComponentInChildren<ScreenShake>().TriggerShake(0.5f, 5f);
            health -= dmg;
            if (health <= 0)
                Die();
            else
                invincibilityFraming = true;
            yield return new WaitForSeconds(invicibilityFrames);
            invincibilityFraming = false;
        }
    }

    public void Die()
    {
        Debug.Log("Die");

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponentInParent<AIPath>().enabled = false;
    }
}
