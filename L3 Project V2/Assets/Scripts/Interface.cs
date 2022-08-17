using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class Interface : MonoBehaviour
{
    public GameObject Canvas;
    private int saveNum = 0;

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

        WriteToJsonFile(Application.persistentDataPath + "/gamesave" + saveNum + ".save", FormSave());
    }

    public void SelectSave(int id)
    {
        if (File.Exists("gamesave" + Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].text + ".save"))
        {
            Debug.Log("Exists");
            LoadSave(Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].text);
        }
        else
        {
            Debug.Log("Doesn't Exist");
            Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].enabled = true;
            Canvas.transform.Find("SaveFile " + id).Find("Confirm").gameObject.SetActive(true);
            Canvas.transform.Find("SaveFile " + id).Find("Cancel").gameObject.SetActive(true);
            GameEvents.current.TxtBoxSelect(id);
        }
    }

    public void ConfirmSave(int id)
    {
        WriteToJsonFile(Application.persistentDataPath + "/gamesave" + Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].text + ".save", new Save());
        Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].enabled = false;
        GameEvents.current.TxtBoxDeselect(id);
        Canvas.transform.Find("SaveFile " + id).Find("Confirm").gameObject.SetActive(false);
        Canvas.transform.Find("SaveFile " + id).Find("Cancel").gameObject.SetActive(false);
    }

    public void DenySave(int id)
    {
        GameEvents.current.SetTxtBoxValue(id, "New Game");
        Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].enabled = false;
        GameEvents.current.TxtBoxDeselect(id);
        Canvas.transform.Find("SaveFile " + id).Find("Confirm").gameObject.SetActive(false);
        Canvas.transform.Find("SaveFile " + id).Find("Cancel").gameObject.SetActive(false);
    }

    private void CreateSave(int id)
    {

    }

    private Save FormSave()
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

    private void LoadSave(string saveName)
    {
        Save save = ReadFromJsonFile<Save>(Application.persistentDataPath + "/gamesave"+saveName+".save");

        player.health = save.health;

        foreach (Boss boss in bosses)
        {
            boss.defeated = true;
        }

        foreach (int boss in save.bossesUndefeated)
        {
            bosses[boss].defeated = false;
        }
    }

    public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter writer = null;
        try
        {
            var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
            writer = new StreamWriter(filePath, append);
            writer.Write(contentsToWriteToFile);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }
    }

    public static T ReadFromJsonFile<T>(string filePath) where T : new()
    {
        TextReader reader = null;
        try
        {
            reader = new StreamReader(filePath);
            var fileContents = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(fileContents);
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }


}
