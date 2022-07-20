using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private bool interactable = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            interactable = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            interactable = false;
    }

    private void Update()
    {
        if (interactable && Input.GetKeyDown(KeyCode.F))
            Interact();
    }

    public virtual void Interact() 
    { 
        // override this
    }
}
