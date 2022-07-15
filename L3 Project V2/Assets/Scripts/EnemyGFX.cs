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

    private int health = 6;
    private float attackingTime;
    private int currentState = 0;
    private float lockedTill = 0;

    private static readonly int Attack = Animator.StringToHash("MosqAttack");
    private static readonly int Fly = Animator.StringToHash("MosqFollow");

    void Update()
    {
        int state=0;
        if (Time.time < lockedTill) return;
        else if (Physics2D.OverlapCircle(transform.position, radius, player) && Time.time > attackingTime)
        {
            aiPath.maxSpeed = 200;
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

    [System.Obsolete]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerWeapon) != 0)
        {
            aiPath.target.GetComponentInChildren<ScreenShake>().TriggerShake(0.5f, 5f);
            health -= collision.gameObject.GetComponent<Weapon>().damage;
            if (health <= 0)
                Die();
        }
    }

    public void Die()
    {
        Debug.Log("Die");
    }
}
