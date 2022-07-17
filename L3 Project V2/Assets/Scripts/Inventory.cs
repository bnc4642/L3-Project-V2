using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private int pageNum = 0;
    private bool flipping;

    public GameObject[] pages = new GameObject[2];

    public Sprite[] heights;
    public GameObject pf;

    public IEnumerator FlipPage(string Dir)
    {
        if (flipping)
        {
            yield return null;
        }
        else if (Dir.Equals("l"))
        {
            if (pageNum > 0)
            {
                flipping = true;
                pf.GetComponent<SpriteRenderer>().enabled = true;
                pf.GetComponent<Animator>().Play("Flip_l", -1, 0);
                yield return new WaitForSeconds(0.23f);
                pf.GetComponent<SpriteRenderer>().enabled = false;
                flipping = false;
                if (pageNum == 1) // red tag
                {
                    transform.GetChild(6).gameObject.SetActive(false);
                    transform.GetChild(7).gameObject.SetActive(true);
                }
                else if (pageNum == 5) // green tag
                {
                    transform.GetChild(2).gameObject.SetActive(false);
                    transform.GetChild(3).gameObject.SetActive(true);
                }
                else if (pageNum == 8) // green tag
                {
                    transform.GetChild(4).gameObject.SetActive(false);
                    transform.GetChild(5).gameObject.SetActive(true);
                }
                pageNum--;
            }
        }
        else 
        {
            if (pageNum < 8)
            {
                flipping = true;
                pf.GetComponent<SpriteRenderer>().enabled = true;
                pf.GetComponent<Animator>().Play("Flip_r", -1, 0);
                yield return new WaitForSeconds(0.23f);
                pf.GetComponent<SpriteRenderer>().enabled = false;
                flipping = false;
                if (pageNum == 0) // red tag
                {
                    transform.GetChild(6).gameObject.SetActive(true);
                    transform.GetChild(7).gameObject.SetActive(false);
                }
                if (pageNum == 4) // green tag
                {
                    transform.GetChild(2).gameObject.SetActive(true);
                    transform.GetChild(3).gameObject.SetActive(false);
                }
                else if (pageNum == 7) // green tag
                {
                    transform.GetChild(4).gameObject.SetActive(true);
                    transform.GetChild(5).gameObject.SetActive(false);
                }
                pageNum++;
            }
        }
        GetComponent<SpriteRenderer>().sprite = heights[pageNum];
    }
}
