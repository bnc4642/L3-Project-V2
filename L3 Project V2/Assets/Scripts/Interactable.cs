using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            if (collision.GetComponent<Player>().interactable == null)
            collision.GetComponent<Player>().interactable = this;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            collision.GetComponent<Player>().interactable = null;
    }

    public virtual void Interact() 
    { 
        // override this
    }
}
