using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : MonoBehaviour
{
    public LayerMask player;
    public LayerMask enemy;
    public bool facingRight = false;
    int dmg = 3;
    private IEnumerator OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemy) != 0)
        {
            if (facingRight)
                collision.gameObject.GetComponent<EnemyGFX>().Hit(dmg, 0);
            else if (!facingRight)
                collision.gameObject.GetComponent<EnemyGFX>().Hit(dmg, 180);
            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(collision.gameObject.GetComponent<EnemyGFX>().attackTime + 0.3f);
            Destroy(this.gameObject);
        }
        else if (((1 << collision.gameObject.layer) & player) == 0)
            Destroy(this.gameObject);

        yield return null;
    }
}
