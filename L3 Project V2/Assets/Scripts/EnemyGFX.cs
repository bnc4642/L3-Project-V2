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

    private float attackingTime;
    private int currentState = 0;
    private float lockedTill = 0;

    private static readonly int Attack = Animator.StringToHash("MosqAttack");
    private static readonly int Fly = Animator.StringToHash("MosqFollow");

    void Update()
    {
        int state;
        if (Time.time < lockedTill) return;
        else if (Physics2D.OverlapCircle(transform.position, radius, player) && Time.time > attackingTime)
        {
            attackingTime = Time.time + attackTime + 0.5f;
            lockedTill = Time.time + attackTime;
            state = Attack;
            Debug.Log("1.                " + state);
        }
        else
        {
            state = Fly;
        }
        Debug.Log("2.                " + state);
        if (aiPath.target.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        Debug.Log("3.                " + state);

        if (state == currentState) { }
        else
        {
            anim.CrossFade(state, 0, 0);
            currentState = state;
        }
    }
}
