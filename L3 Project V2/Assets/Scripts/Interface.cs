using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Interface : MonoBehaviour
{
    private int Id = 0;

    public GameObject Canvas;
    public GameObject ExCanvas;
    public GameObject BookCanvas;
    private int saveNum = 0;

    private int pageNum = 0;
    private bool flipping;

    public GameObject[,] pages = new GameObject[2, 9];

    public Sprite[] heights;
    public GameObject pf;

    private List<Boss> bosses = new List<Boss>();
    public Player player;

    public void OnEnter(InputValue value)
    {
        ConfirmSave(Id);
    }

    public void PreFlipPage(string dir)
    {
        StartCoroutine(FlipPage(dir));
    }

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
                    BookCanvas.transform.GetChild(6).gameObject.SetActive(false);
                    BookCanvas.transform.GetChild(7).gameObject.SetActive(true);
                }
                else if (pageNum == 5) // green tag
                {
                    BookCanvas.transform.GetChild(2).gameObject.SetActive(false);
                    BookCanvas.transform.GetChild(3).gameObject.SetActive(true);
                }
                else if (pageNum == 8) // green tag
                {
                    BookCanvas.transform.GetChild(4).gameObject.SetActive(false);
                    BookCanvas.transform.GetChild(5).gameObject.SetActive(true);
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
                    BookCanvas.transform.GetChild(6).gameObject.SetActive(true);
                    BookCanvas.transform.GetChild(7).gameObject.SetActive(false);
                }
                else if (pageNum == 4) // green tag
                {
                    BookCanvas.transform.GetChild(2).gameObject.SetActive(true);
                    BookCanvas.transform.GetChild(3).gameObject.SetActive(false);
                }
                else if (pageNum == 7) // green tag
                {
                    BookCanvas.transform.GetChild(4).gameObject.SetActive(true);
                    BookCanvas.transform.GetChild(5).gameObject.SetActive(false);
                }
                pageNum++;
            }
        }
        GetComponent<SpriteRenderer>().sprite = heights[pageNum];

        //WriteToJsonFile(Application.persistentDataPath + "/gamesave" + saveNum + ".save", FormSave());
    }

    public void SelectSave(int id)
    {
        if (File.Exists("gamesave" + id + ".save"))
        {
            Debug.Log("Exists");
            LoadSave(id);
        }
        else
        {
            Debug.Log("Doesn't Exist");
            Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].enabled = true;
            ExCanvas.transform.Find("" + id).Find("Confirm").gameObject.SetActive(true);
            ExCanvas.transform.Find("" + id).Find("Cancel").gameObject.SetActive(true);
            Canvas.transform.Find("SaveFile " + id).GetComponent<Button>().enabled = false;
            GameEvents.current.TxtBoxSelect(id);
        }
    }

    public void ConfirmSave(int id)
    {
        Debug.Log("A");
        WriteToJsonFile(Application.persistentDataPath + "/gamesave" + id + ".save", new Save());
        ExitSaveTyping(id);
    }

    public void DenySave(int id)
    {
        Debug.Log("B");
        GameEvents.current.SetTxtBoxValue(id, "New Game");
        ExitSaveTyping(id);
    }
    public void SelectingID(int ID) { Id = ID; }

    public void ExitSaveTyping(int id)
    {
        Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].enabled = false;
        ExCanvas.transform.Find("" + id).Find("Confirm").gameObject.SetActive(false);
        ExCanvas.transform.Find("" + id).Find("Cancel").gameObject.SetActive(false);
        Canvas.transform.Find("SaveFile " + id).GetComponent<Button>().enabled = true;
        GameEvents.current.TxtBoxDeselect(id);
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

    private void LoadSave(int id)
    {
        Save save = ReadFromJsonFile<Save>(Application.persistentDataPath + "/gamesave"+id+".save");

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

    public void ChangeScene()
    {
        Canvas.gameObject.SetActive(false);
        ExCanvas.gameObject.SetActive(false);

        GetComponent<SpriteRenderer>().enabled = true;
        BookCanvas.SetActive(true);
    }
}
