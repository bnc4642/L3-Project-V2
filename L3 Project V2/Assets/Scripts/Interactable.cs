using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public List<string> Dialogue = new List<string>();
    public int dialogueNums = 0;
    public string ImpactfulNums = "";
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

    public virtual void DialogImpact()
    {
        // move some stuff, and add conditions to scenes when they load;
        // switch case style
    }
}