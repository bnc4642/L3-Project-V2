using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private int pageNum = 0;
    private bool flipping;

    public GameObject[,] pages = new GameObject[2, 9];

    public Sprite[] heights;
    public GameObject pf;

    private List<Boss> bosses = new List<Boss>();
    public Player player;

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
                else if (pageNum == 4) // green tag
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

    private Save CreateSaveGameObject()
    {
        Save save = new Save();
        foreach (Boss boss in bosses)
        {
            if (!boss.defeated)
                save.bossesUndefeated.Add(boss.bossNum);
        }

        save.health = player.health;

        return save;
    }

    public void SaveGame()
    {
        Save save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, save);
        file.Close();
    }

    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {


            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();

            foreach (Boss boss in bosses)
            {
                boss.defeated = true;
            }
            for (int i = 0; i < save.bossesUndefeated.Count; i++)
            {
                bosses[i].defeated = false;
            }

            player.health = save.health;

            // etc.
        }
        else
        {
            Debug.Log("No game saved!");
        }
    }
}
