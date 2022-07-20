using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : MonoBehaviour
{
    public LayerMask player;
    public LayerMask enemy;
    private bool facingRight = false;
    const int dmg = 3;
    private IEnumerator OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemy) != 0)
        {
            StartCoroutine(collision.gameObject.GetComponent<Mosquito>().Hit(dmg, Convert.ToInt32(facingRight)));
            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(collision.gameObject.GetComponent<Mosquito>().timeForAttack + 0.3f);
            Destroy(this.gameObject);
        }
        else if (((1 << collision.gameObject.layer) & player) == 0)
            Destroy(this.gameObject);

        yield return null;
    }

    public bool FacingRight
    {
        get { return facingRight; }
        set { facingRight = value; }
    }
}
