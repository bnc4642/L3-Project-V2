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
    private Save[] Saves = new Save[3];

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

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            Saves[i] = ReadFromJsonFile<Save>(Application.persistentDataPath + "/gamesave" + i + ".save"); // get saves
            if (Saves[i] == null || Saves[i].Name == "")
            {
                Debug.Log("Not Safe");
                Saves[i] = null;
                ExCanvas.transform.GetChild(i).GetComponentInChildren<TMPro.TMP_Text>().text = "New Game";
            }
            else
            {
                Debug.Log("Safe");
                ExCanvas.transform.GetChild(i).GetComponentInChildren<TMPro.TMP_Text>().text = Saves[i].Name;
            }
        }
    }
    public void OnEnter(InputValue value)
    {
        ConfirmSave(Id); // should this be the only method of confirming? Probably not
    }

    public void PreFlipPage(string dir)
    {
        StartCoroutine(FlipPage(dir)); // this is so that the IEnumerator can be called
    }

    private IEnumerator FlipPage(string Dir)
    {
        if (flipping)
            yield return null; // if in the middle of flipping

        else if (Dir.Equals("l")) // if turning to the left
        {
            if (pageNum > 0) // if navigating through the book
            {
                flipping = true;
                pf.GetComponent<SpriteRenderer>().enabled = true; // show the animation of the page being flipped
                pf.GetComponent<Animator>().Play("Flip_l", -1, 0);
                yield return new WaitForSeconds(0.23f);
                pf.GetComponent<SpriteRenderer>().enabled = false; // hide the animation after page has been flipped
                flipping = false;
                if (pageNum == 1) // move the red tag
                {
                    BookCanvas.transform.GetChild(6).gameObject.SetActive(false);
                    BookCanvas.transform.GetChild(7).gameObject.SetActive(true);
                }
                else if (pageNum == 5) // move the green tag
                {
                    BookCanvas.transform.GetChild(2).gameObject.SetActive(false);
                    BookCanvas.transform.GetChild(3).gameObject.SetActive(true);
                }
                else if (pageNum == 8) // move the green tag
                {
                    BookCanvas.transform.GetChild(4).gameObject.SetActive(false);
                    BookCanvas.transform.GetChild(5).gameObject.SetActive(true);
                }
                pageNum--;
            }
            else // if closing the book
            {
                pageNum--;
                GetComponent<Animator>().enabled = true; // animate the closing
                GetComponent<Animator>().SetTrigger("Close"+Id);
                BookCanvas.transform.GetChild(0).gameObject.SetActive(false); // hide the navigation buttons
                for (int i = 1; i < 8; i+=2)
                    BookCanvas.transform.GetChild(i).gameObject.SetActive(false);
                yield return new WaitForSeconds(0.33f);
                BookCanvas.transform.GetChild(8).gameObject.SetActive(true); // show the navigation button in the correct place, and show the return and delete buttons
                BookCanvas.transform.GetChild(12).gameObject.SetActive(true);
                BookCanvas.transform.GetChild(13).gameObject.SetActive(true);
                ExCanvas.transform.GetChild(Id).gameObject.SetActive(true); // show the book button
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
                ExCanvas.transform.GetChild(Id).gameObject.SetActive(false);
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
                if (pageNum == 0) // move the red tag
                {
                    BookCanvas.transform.GetChild(6).gameObject.SetActive(true);
                    BookCanvas.transform.GetChild(7).gameObject.SetActive(false);
                }
                else if (pageNum == 4) // move the green tag
                {
                    BookCanvas.transform.GetChild(2).gameObject.SetActive(true);
                    BookCanvas.transform.GetChild(3).gameObject.SetActive(false);
                }
                else if (pageNum == 7) // move the green tag
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
        if (Canvas.transform.GetChild(id).GetChild(0).GetComponentsInChildren<TMPro.TextMeshProUGUI>()[1].text != "") // Check not null
        {
            WriteToJsonFile(Application.persistentDataPath + "/gamesave" + id + ".save", FormSave(5, 0));
            ExitSaveTyping(id, Canvas.transform.GetChild(id).GetComponentsInChildren<TMPro.TMP_Text>()[1].text);
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

        Canvas.transform.GetChild(id).gameObject.SetActive(false); // Swap text boxes from editable to non-editable
        GameEvents.current.TxtBoxDeselect(id);
        ExCanvas.transform.GetChild(id).GetChild(0).gameObject.SetActive(true);
        ExCanvas.transform.GetChild(id).GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
    }

    private Save FormSave(int health, int encounterProgress) // should be called upon pressing esc, saving the world, and upon random points. The values are updated upon changes like interactions.
    {
        // if any noticable events occured, then save upon closing the book (PD < 0)
        Save save = new Save();

        Debug.Log(Saves[Id] == null);
        if (Saves[Id] == null)
        {
            save = new Save();
            save.Name = Canvas.transform.GetChild(Id).GetComponentsInChildren<TMPro.TMP_Text>()[1].text;
        }
        else
            save = Saves[Id];

        foreach (Boss boss in bosses)
        {
            if (!boss.defeated)
                save.EncounterProgress = encounterProgress;
        }

        save.Health = health;

        return save;
    }

    private void LoadSave(int id) // should be called upon entering the game. It forms the world.
    {
        //player.health = Saves[id].Health;
        // close the book, and create an instance of the inventory(values)
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
        catch
        {
            Debug.Log("Failed");
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
            Debug.Log(fileContents);
            return JsonConvert.DeserializeObject<T>(fileContents);
        }
        catch
        {
            return new T();
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
