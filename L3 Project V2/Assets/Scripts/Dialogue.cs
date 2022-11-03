using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    //variables
    public TMPro.TMP_Text Text;
    private char side = 'L';
    public SpriteRenderer Portrait;
    public TMPro.TMP_Text DialogueName;

    void Start()
    {
        //get variables and stop animations from showing until interaction starts
        GetComponent<Animator>().speed = 0;
        Text = GetComponentInChildren<TMPro.TMP_Text>();
    }

    public IEnumerator MoveDialogue(char direction) //direction is +/- 1 depending on moving forward or backward
    {
        GetComponent<Animator>().Play("Dialogue" + direction + side, -1, 0); //initiates animation with custom side and direction
        GetComponent<Animator>().speed = 1;
        yield return new WaitForSeconds(0.85f); //once animation finishes, stop animator
        GetComponent<Animator>().speed = 0;
    }

    public IEnumerator SwitchDialogue(Sprite pic, string Name, Player p)
    {
        //start animation
        GetComponent<Animator>().Play("DialogueOpen"+side, -1, 0);
        GetComponent<Animator>().speed = 1;
        yield return new WaitForSeconds(0.3f);
        //changes these after animation is complete
        Portrait.sprite = pic;
        DialogueName.text = Name;
        yield return new WaitForSeconds(0.3f);
        //sawp sides
        if (side == 'L') side = 'R';
        else side = 'L';
        //stop dialogue and animations
        p.StopSwitchingDialogue();
        GetComponent<Animator>().speed = 0;
    }

    public void ResetSide() //on start of interaction
    {
        side = 'L';
    }

    public void FirstNameAndPicture(Sprite pic, string Name) //initial images
    {
        Portrait.sprite = pic;
        DialogueName.text = Name;
    }
}
