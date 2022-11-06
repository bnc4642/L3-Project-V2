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
    //variables
    private Save[] saves = new Save[3];

    private int id = 0;

    public int pageNum = -1;
    private bool flipping;
    private bool tabBtnPressed = false;

    public GameObject Canvas;
    public GameObject ExCanvas;
    public GameObject BookCanvas;

    public List<GameObject> Pages = new List<GameObject>();
    public GameObject PageFlipper;

    public Sprite[] BookSprites;
    public GameObject TaskObject;
    private List<Boss> bosses = new List<Boss>(); //for the save file

    private Vector2[] taskPosition = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), };


    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            saves[i] = ReadFromJsonFile<Save>(Application.persistentDataPath + "/gamesave" + i + ".save"); // get saves
            if (saves[i] == null || saves[i].Name == "") //if save doesn't exist
            {
                saves[i] = null; //empty save slot
                ExCanvas.transform.GetChild(i).GetComponentInChildren<TMPro.TMP_Text>().text = "New Game"; //set name to "New Game"
            }
            else
                ExCanvas.transform.GetChild(i).GetComponentInChildren<TMPro.TMP_Text>().text = saves[i].Name; //set name to name of save
        }
    }
    public void OnEnter(InputValue value) //called upon pressing enter while in an input box
    {
        ConfirmSave(id); // should this be the only method of confirming? Probably not
    }

    public void PreFlipPage(string dir) //called from the input box
    {
        StartCoroutine(FlipPage(dir, true)); // this is so that the IEnumerator can be called
    }

    private IEnumerator FlipPage(string Dir, bool displayingPages)
    {
        if (flipping && !tabBtnPressed)
            yield return null; // if in the middle of flipping

        GetComponent<Animator>().speed = 1;

        if (Dir.Equals("l") && pageNum == 0) //close book
        {
            pageNum--;
            GetComponent<Animator>().enabled = true; // animate the closing
            GetComponent<Animator>().SetTrigger("Close"+id);
            BookCanvas.transform.GetChild(0).gameObject.SetActive(false); // hide the navigation buttons
            Pages[0].gameObject.SetActive(false);
            Pages[1].gameObject.SetActive(false);
            for (int i = 1; i < 8; i += 2)
                BookCanvas.transform.GetChild(i).gameObject.SetActive(false);
            yield return new WaitForSeconds(0.33f);
            GetComponent<Animator>().speed = 0;
            BookCanvas.transform.GetChild(8).gameObject.SetActive(true); // show the navigation button in the correct place, and show the return and delete buttons
            BookCanvas.transform.GetChild(12).gameObject.SetActive(true);
            BookCanvas.transform.GetChild(13).gameObject.SetActive(true);
            ExCanvas.transform.GetChild(id).gameObject.SetActive(true); // show the book button
        }
        else if (Dir.Equals("r") && pageNum < 0) //open book
        {
            GetComponent<Animator>().SetTrigger("Open" + id);
            GetComponent<Animator>().speed = 1;
            BookCanvas.transform.GetChild(8).gameObject.SetActive(false); //remove options like delete save
            BookCanvas.transform.GetChild(12).gameObject.SetActive(false);
            BookCanvas.transform.GetChild(13).gameObject.SetActive(false);
            ExCanvas.transform.GetChild(id).gameObject.SetActive(false);
            pageNum++;
            yield return new WaitForSeconds(0.33f);
            GetComponent<Animator>().enabled = false;

            BookCanvas.transform.GetChild(0).gameObject.SetActive(true); //display the navigation buttons
            for (int i = 1; i < 8; i += 2)
                BookCanvas.transform.GetChild(i).gameObject.SetActive(true);

            Pages[0].gameObject.SetActive(true);
            Pages[1].gameObject.SetActive(true);
        }
        else if ((pageNum > 0 && Dir.Equals("l")) || (pageNum < 7 && Dir.Equals("r"))) // if navigating through the book
        {
            int LeftInput = 0;
            if (Dir.Equals("l")) { LeftInput = 1; }
            bool removing = Dir.Equals("l"); //if turning left

            Pages[2 * (pageNum)].gameObject.SetActive(false);
            Pages[2 * (pageNum) + 1].gameObject.SetActive(false);
            flipping = true;
            PageFlipper.GetComponent<SpriteRenderer>().enabled = true; // show the animation of the page being flipped
            PageFlipper.GetComponent<Animator>().Play("Flip_"+Dir, -1, 0);
            yield return new WaitForSeconds(0.23f);
            GetComponent<Animator>().speed = 0;
            PageFlipper.GetComponent<SpriteRenderer>().enabled = false; // hide the animation after page has been flipped
            flipping = false;
            if (pageNum == 0 + LeftInput) // move the red tag
            {
                BookCanvas.transform.GetChild(6).gameObject.SetActive(!removing);
                BookCanvas.transform.GetChild(7).gameObject.SetActive(removing);
            }
            else if (pageNum == 1 + LeftInput) // move the green tag
            {
                BookCanvas.transform.GetChild(2).gameObject.SetActive(!removing);
                BookCanvas.transform.GetChild(3).gameObject.SetActive(removing);
            }
            else if (pageNum == 2 + LeftInput) // move the green tag
            {
                BookCanvas.transform.GetChild(4).gameObject.SetActive(!removing);
                BookCanvas.transform.GetChild(5).gameObject.SetActive(removing);
            }

            if (removing)
                pageNum--;
            else
                pageNum++;

            if (displayingPages)
            {
                Pages[2 * (pageNum)].gameObject.SetActive(true);
                Pages[2 * (pageNum) + 1].gameObject.SetActive(true);
            }
        }
        if (pageNum >= 0 && pageNum<=8)
            GetComponent<SpriteRenderer>().sprite = BookSprites[pageNum];
    }

    public void BookTag(int pageNumIntended) //controls for book tabs
    {
        int n = pageNum - pageNumIntended;
        if (n < 0) //going right
        {
            tabBtnPressed = true;
            for (int i = 0; i > n; i--)
            {
                StartCoroutine(FlipPage("r", i == n + 1));
            }
        }
        else if (n > 0) //going left
        {
            tabBtnPressed = true;
            for (int i = 0; i < n - 1; i++)
                StartCoroutine(FlipPage("l", i == n - 2));
        }

        tabBtnPressed = false; //reset btn
    }

    public void SelectSave(int ID) //upon selection
    {
        id = ID;
        GM.Instance.saveID = id;
        if (File.Exists(Application.persistentDataPath + "/gamesave" + ID + ".save")) // save exists
        {
            for (int i = 0; i < 3; i++)
                ExCanvas.transform.Find("Button " + i).gameObject.SetActive(false); //remove all buttons
            ExCanvas.transform.Find("Button " + ID).gameObject.SetActive(true);//except the selected one
            ExCanvas.transform.Find("Button " + ID).GetComponent<Button>().enabled = false; //but disable button abilities

            //begin but pause animations
            GetComponent<Animator>().enabled = true;
            GetComponent<Animator>().SetTrigger("Open" + id);
            GetComponent<Animator>().speed = 0;

            //set this object's position
            transform.position = new Vector2((ID-1) * 11.65f, -3.2f);

            //set out buttons properly and display this object to prepare for the animations
            GetComponent<SpriteRenderer>().enabled = true;
            BookCanvas.transform.GetChild(12).gameObject.SetActive(true);
            BookCanvas.transform.GetChild(13).gameObject.SetActive(true);
            Vector2 btnPos = BookCanvas.transform.GetChild(8).GetComponent<RectTransform>().localPosition;
            btnPos.x = 15.3f + (ID - 1) * 32.12f;
            BookCanvas.transform.GetChild(8).GetComponent<RectTransform>().localPosition = btnPos;
            BookCanvas.transform.GetChild(8).gameObject.SetActive(true);

            LoadSave(ID);
            FillBook();
        }
        else // new file
        {
            Canvas.transform.Find("Name " + ID).gameObject.SetActive(true); //enable input box
            foreach (Transform btn in ExCanvas.GetComponentsInChildren<Transform>())
                if (btn.gameObject.GetComponent<Button>() != null)
                    btn.gameObject.GetComponent<Button>().enabled = false; //disable buttons
            ExCanvas.transform.Find("Button " + ID).GetChild(0).gameObject.SetActive(false); //hide text of book
            GameEvents.current.TxtBoxSelect(ID);
        }
    }

    public void ConfirmSave(int ID) //exiting input box
    {
        if (Canvas.transform.GetChild(ID).GetChild(0).GetComponentsInChildren<TMPro.TextMeshProUGUI>()[1].text != "") // Check not null
        {
            //create empty save file
            WriteToJsonFile(Application.persistentDataPath + "/gamesave" + ID + ".save", FormSave(5, 0));
            ExitSaveTyping(ID, Canvas.transform.GetChild(ID).GetComponentsInChildren<TMPro.TMP_Text>()[1].text);
        }
        else
            DenySave(ID);
    }

    public void DenySave(int ID) // if save creation process is exited
    {
        GameEvents.current.SetTxtBoxValue(ID, "New Game");
        ExitSaveTyping(ID, "New Game");
    }

    public void ExitSaveTyping(int ID, string name)
    {
        foreach (Transform btn in ExCanvas.GetComponentsInChildren<Transform>())
            if (btn.gameObject.GetComponent<Button>() != null)
                btn.gameObject.GetComponent<Button>().enabled = true; //enable buttons again

        Canvas.transform.GetChild(ID).gameObject.SetActive(false); // Swap text boxes from editable to non-editable
        GameEvents.current.TxtBoxDeselect(ID);
        ExCanvas.transform.GetChild(ID).GetChild(0).gameObject.SetActive(true);
        ExCanvas.transform.GetChild(ID).GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name; //display and set text on button
    }

    private Save FormSave(int health, int encounterProgress) // should be called upon pressing esc, saving the world, and upon random points. The values are updated upon changes like interactions.
    {
        // if any noticable events occured, then save upon closing the book (PD < 0)
        Save save = new Save();

        if (saves[id] == null) //if save doesn't exist, create one and set name
        {
            save = new Save();
            save.Name = Canvas.transform.GetChild(id).GetComponentsInChildren<TMPro.TMP_Text>()[1].text;
        }
        else
            save = saves[id];

        //set important variables within save to be saved

        foreach (Boss boss in bosses)
        {
            if (!boss.defeated)
                save.EncounterProgress = encounterProgress;
        } 

        save.Health = health;

        saves[id] = save;

        return save;
    }

    private void LoadSave(int ID) // should be called upon entering the game. It forms the world.
    {
        //player.health = saves[ID].Health;
        // close the book, and create an instance of the inventory(values)
    }

    public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new() //physically save the save files onto device
    {
        TextWriter writer = null;
        try
        {
            var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite); //convert the file
            writer = new StreamWriter(filePath, append);
            writer.Write(contentsToWriteToFile); //save the converted file
        }
        finally
        {
            if (writer != null)
                writer.Close(); //close the writer
        }
    }

    public static T ReadFromJsonFile<T>(string filePath) where T : new() //read the physically saved files
    {
        TextReader reader = null;
        try //need try in case files don't exist
        {
            reader = new StreamReader(filePath);
            var fileContents = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(fileContents); //return converted file
        }
        catch
        {
            return new T(); //create a new empy save file 
        }
        finally
        {
            if (reader != null)
                reader.Close(); //close reader
        }
    }

    public void ChangeScene()
    {
        //disable canvases
        Canvas.gameObject.SetActive(false);
        ExCanvas.gameObject.SetActive(false);

        GetComponent<SpriteRenderer>().enabled = true;
        BookCanvas.SetActive(true);
    }

    public void Return()
    {
        ExCanvas.SetActive(true);
        for (int i = 0; i < 3; i++) //enable buttons
            ExCanvas.transform.Find("Button " + i).gameObject.SetActive(true);
        ExCanvas.transform.Find("Button " + id).GetComponent<Button>().enabled = true;

        GetComponent<SpriteRenderer>().enabled = false;
        BookCanvas.transform.GetChild(8).gameObject.SetActive(false); //hide navigation buttons
        BookCanvas.transform.GetChild(12).gameObject.SetActive(false);
        BookCanvas.transform.GetChild(13).gameObject.SetActive(false);
    }
    public void DeleteSave()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave" + id + ".save")) //delete save
            File.Delete(Application.persistentDataPath + "/gamesave" + id + ".save");
        ExCanvas.transform.GetChild(id).GetComponentInChildren<TMPro.TMP_Text>().text = "New Game"; //reset name
        GameEvents.current.SetTxtBoxValue(id, "New Game");
        saves[id] = null;
        Return();
    }

    private void FillBook()
    {
        TMPro.TextMeshProUGUI[] skillboxes = Pages[2].GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        skillboxes[0].text = saves[id].Name; //set name

        skillboxes[2].text = "";
        for (int i = 0; i < 6; i++)
            if (saves[id].Skills[i, 1] == "1")
                skillboxes[2].text += saves[id].Skills[i, 0] + "\n"; //set skills

        Pages[3].GetComponentsInChildren<TMPro.TextMeshProUGUI>()[2].text = "";
        for (int i = 0; i < 6; i++)
            foreach (var t in saves[id].Inventory)
                Pages[3].GetComponentsInChildren<TMPro.TextMeshProUGUI>()[2].text += t.GetType().Name + "\n"; //set inventory


        foreach (char scene in saves[id].mapList.ToCharArray())
        {
            int n = Int32.Parse(scene.ToString());
            if (n > 9)
                Pages[4].transform.GetChild(n-9).gameObject.SetActive(true);
            else if (n < 9)
                Pages[5].transform.GetChild(n).gameObject.SetActive(true);
            else
            {
                Pages[4].transform.GetChild(0).gameObject.SetActive(true);
                Pages[5].transform.GetChild(n).gameObject.SetActive(true);
            }
        }

        int taskCounter = 0;
        int secondaryCounter = 6;
        foreach (Task task in GM.Instance.TaskManager.Tasks)
        {
            GameObject t = Instantiate(TaskObject);
            t.transform.position = taskPosition[taskCounter];
            t.transform.parent = Pages[secondaryCounter].transform;
            TMPro.TextMeshProUGUI[] texts = t.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            texts[0].text = task.Title;
            texts[1].text = task.Description;

            if (taskCounter < 4) taskCounter++;
            else
            {
                secondaryCounter++;
                taskCounter = 0;
            }
        }
    }
}
