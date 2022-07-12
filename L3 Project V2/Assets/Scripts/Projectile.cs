using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask player;
    public LayerMask enemy;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemy) != 0)
        {
            Destroy(this.gameObject);
            // Damage the enemy
        }
        else if (((1 << collision.gameObject.layer) & player) == 0)
            Destroy(this.gameObject);
    }
}
