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

    private int pageNum = -1;
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
            else
            {
                pageNum--;
                GetComponent<Animator>().enabled = true;
                GetComponent<Animator>().SetTrigger("Close"+Id);
                BookCanvas.transform.GetChild(3).gameObject.SetActive(false);
                BookCanvas.transform.GetChild(5).gameObject.SetActive(false);
                BookCanvas.transform.GetChild(7).gameObject.SetActive(false);
                BookCanvas.transform.GetChild(0).gameObject.SetActive(false);
                BookCanvas.transform.GetChild(1).gameObject.SetActive(false);
                yield return new WaitForSeconds(0.33f);
                BookCanvas.transform.GetChild(8).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(12).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(13).gameObject.SetActive(true);
            }
        }
        else
        {
            if (pageNum < 0)
            {
                GetComponent<Animator>().SetTrigger("Open"+Id);
                GetComponent<Animator>().speed = 1;
                BookCanvas.transform.GetChild(8).gameObject.SetActive(false);
                BookCanvas.transform.GetChild(12).gameObject.SetActive(false);
                BookCanvas.transform.GetChild(13).gameObject.SetActive(false);
                pageNum++;
                yield return new WaitForSeconds(0.33f);
                GetComponent<Animator>().enabled = false;
                BookCanvas.transform.GetChild(3).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(5).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(7).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(0).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(1).gameObject.SetActive(true);
            }
            else if (pageNum < 8)
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
        if (pageNum >= 0)
            GetComponent<SpriteRenderer>().sprite = heights[pageNum];

        //WriteToJsonFile(Application.persistentDataPath + "/gamesave" + saveNum + ".save", FormSave());
    }

    public void SelectSave(int id)
    {
        Debug.Log("Selecting");
        Id = id;
        if (File.Exists(Application.persistentDataPath + "/gamesave" + id + ".save"))
        {
            ExCanvas.transform.Find("Button " + 0).gameObject.SetActive(false);
            ExCanvas.transform.Find("Button " + 1).gameObject.SetActive(false);
            ExCanvas.transform.Find("Button " + 2).gameObject.SetActive(false);
            ExCanvas.transform.Find("Button " + id).gameObject.SetActive(true);
            ExCanvas.transform.Find("Button " + id).GetComponent<Button>().enabled = false;

            GetComponent<Animator>().enabled = true;
            GetComponent<Animator>().SetTrigger("Open" + Id);
            GetComponent<Animator>().speed = 0;

            transform.position = new Vector2((id-1) * 13, -3.2f);

            GetComponent<SpriteRenderer>().enabled = true;
            BookCanvas.transform.GetChild(12).gameObject.SetActive(true);
            BookCanvas.transform.GetChild(13).gameObject.SetActive(true);
            Vector2 btnPos = BookCanvas.transform.GetChild(8).GetComponent<RectTransform>().localPosition;
            btnPos.x = 15.3f + (id - 1) * 32.12f;
            BookCanvas.transform.GetChild(8).GetComponent<RectTransform>().localPosition = btnPos;
            BookCanvas.transform.GetChild(8).gameObject.SetActive(true);

            LoadSave(id);
        }
        else
        {
            Canvas.transform.Find("Name " + id).gameObject.SetActive(true);
            foreach (Transform btn in ExCanvas.GetComponentsInChildren<Transform>())
            {
                if (btn.gameObject.GetComponent<Button>() != null)
                {
                    btn.gameObject.GetComponent<Button>().enabled = false;
                }
            }
            ExCanvas.transform.Find("Button " + id).GetChild(0).gameObject.SetActive(false);
            GameEvents.current.TxtBoxSelect(id);
        }
    }

    public void ConfirmSave(int id)
    {
        if (Canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[id].text != "") // Check not null
        {
            WriteToJsonFile(Application.persistentDataPath + "/gamesave" + id + ".save", new Save());
            ExitSaveTyping(id, Canvas.GetComponentsInChildren<TMPro.TMP_Text>()[id].text);
        }
    }

    public void DenySave(int id)
    {
        GameEvents.current.SetTxtBoxValue(id, "New Game");
        ExitSaveTyping(id, "New Game");
    }
    public void ExitSaveTyping(int id, string name)
    {
        foreach (Transform btn in ExCanvas.GetComponentsInChildren<Transform>())
        {
            if (btn.gameObject.GetComponent<Button>() != null)
            {
                btn.gameObject.GetComponent<Button>().enabled = true;
            }
        }
        ExCanvas.transform.Find("Button " + id).GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
        Canvas.transform.Find("Name " + id).gameObject.SetActive(true); // Swap text boxes from editable to non-editable
        Canvas.GetComponentsInChildren<TMPro.TMP_InputField>()[id].enabled = false;
        GameEvents.current.TxtBoxDeselect(id);
    }

    private Save FormSave()
    {
        // if any noticable events occured, then save upon closing the book (PD < 0)
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

    public void Return()
    {
        ExCanvas.SetActive(true);
        for (int i = 0; i < 3; i++)
            ExCanvas.transform.Find("Button " + i).gameObject.SetActive(true);
        ExCanvas.transform.Find("Button " + Id).GetComponent<Button>().enabled = true;

        GetComponent<SpriteRenderer>().enabled = false;
        BookCanvas.transform.GetChild(8).gameObject.SetActive(false);
        BookCanvas.transform.GetChild(12).gameObject.SetActive(false);
        BookCanvas.transform.GetChild(13).gameObject.SetActive(false);
    }
    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave" + Id + ".save"))
            File.Delete(Application.persistentDataPath + "/gamesave" + Id + ".save");
    }
}
