using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask player;
    public LayerMask enemy;
    public int dmg = 3;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemy) != 0)
        {
            collision.gameObject.GetComponent<EnemyGFX>().Hit(dmg);
            Destroy(this.gameObject);
        }
        else if (((1 << collision.gameObject.layer) & player) == 0)
            Destroy(this.gameObject);
    }
}
