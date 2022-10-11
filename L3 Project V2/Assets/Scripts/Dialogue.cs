using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public TMPro.TMP_Text text;
    private char side = 'L';

    void Start()
    {
        GetComponent<Animator>().speed = 0;
        text = GetComponentInChildren<TMPro.TMP_Text>();
    }

    public IEnumerator MoveDialogue(char direction) //direction is +/- 1 depending on moving forward or backward
    {
        GetComponent<Animator>().Play("Dialogue" + direction, -1, 0);
        GetComponent<Animator>().speed = 1;
        yield return new WaitForSeconds(0.85f);
        GetComponent<Animator>().speed = 0;
    }

    public IEnumerator SwitchDialogue(Sprite pic, string name, Player p) // change these after animation is complete
    {
        GetComponent<Animator>().Play("DialogueOpen"+side, -1, 0);
        GetComponent<Animator>().speed = 1;
        yield return new WaitForSeconds(0.6f);
        if (side == 'L') side = 'R';
        else side = 'L';

        p.StopSwitchingDialogue();
        GetComponent<Animator>().speed = 0;
    }

    public void ResetSide()
    {
        side = 'L';
    }
}
