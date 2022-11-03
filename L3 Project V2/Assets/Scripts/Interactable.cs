using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    //variables
    public List<string> Dialogue = new List<string>();
    public int DialogueNums = 0;
    public string ImpactfulNums = ""; //the numbers of interactions that will alter stuff within the game

    private void OnTriggerStay2D(Collider2D collision) //there may be multiple triggers that the player enters, so they will all compete for the spot until all have interacted
    {
        if (collision.CompareTag("Player"))
            if (collision.GetComponent<Player>().Interactable == null)
                collision.GetComponent<Player>().Interactable = this;
    }

    private void OnTriggerExit2D(Collider2D collision) //upon exiting the interactable is removed
    {
        if (collision.CompareTag("Player"))
            if (collision.GetComponent<Player>().Interactable == this)
                collision.GetComponent<Player>().Interactable = null;
    }

    public virtual void DialogImpact() //is called upon impactful numbers of interactions
    {
        // move some stuff, and add conditions to scenes when they load;
        // switch case style
        //custom for each interaction script
    }
}