using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class Ventory : MonoBehaviour
{
    //variables
    public int id = 0;

    public int pageNum = 0;
    private bool flipping;
    private bool tabBtnPressed = false;

    public GameObject BookCanvas;

    public List<GameObject> Pages = new List<GameObject>();
    public GameObject PageFlipper;

    public Sprite[] BookSprites;
    public GameObject TaskObject;
    private List<Boss> bosses = new List<Boss>(); //for the save file

    private Vector2[] taskPosition = new Vector2[4] { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), };


    private void Start()
    {
        TMPro.TextMeshProUGUI[] skillboxes = Pages[2].GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        skillboxes[0].text = GM.Instance.Save.Name; //set name

        skillboxes[2].text = "";
        for (int i = 0; i < 6; i++)
            if (GM.Instance.Save.Skills[i, 1] == "1")
                skillboxes[2].text += GM.Instance.Save.Skills[i, 0] + "\n"; //set skills

        Pages[3].GetComponentsInChildren<TMPro.TextMeshProUGUI>()[2].text = "";
        for (int i = 0; i < 6; i++)
            foreach (var t in GM.Instance.Save.Inventory)
                Pages[3].GetComponentsInChildren<TMPro.TextMeshProUGUI>()[2].text += t.GetType().Name + "\n"; //set inventory


        foreach (Transform location in Pages[4].transform.GetComponentsInChildren<Transform>())
            location.gameObject.SetActive(false);
        foreach (Transform location in Pages[5].transform.GetComponentsInChildren<Transform>())
            location.gameObject.SetActive(false);

        foreach (char scene in GM.Instance.Save.mapList.ToCharArray())
        {
            int n = Int32.Parse(scene.ToString());
            if (n > 9)
                Pages[4].transform.GetChild(n - 9).gameObject.SetActive(true);
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


        //set this object's position
        Vector3 pos = GameObject.Find("NewPixelCamera 1").transform.position;
        transform.position = pos;
        Pages[4].GetComponentInParent<Transform>().position = pos;


        StartCoroutine(StartBtns());
    }

    private IEnumerator StartBtns()
    {
        yield return new WaitForSeconds(0.32f);

        BookCanvas.transform.GetChild(0).gameObject.SetActive(true);
        BookCanvas.transform.GetChild(1).gameObject.SetActive(true);
        for (int i = 3; i < 8; i += 2)
            BookCanvas.transform.GetChild(i).gameObject.SetActive(true);

        BookCanvas.transform.GetChild(12).gameObject.SetActive(true);
        BookCanvas.transform.GetChild(13).gameObject.SetActive(true);


        Pages[0].gameObject.SetActive(true);
        Pages[1].gameObject.SetActive(true);
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
            Debug.Log("Unload scene");
        }
        if ((pageNum > 0 && Dir.Equals("l")) || (pageNum < 7 && Dir.Equals("r"))) // if navigating through the book
        {
            int LeftInput = 0;
            if (Dir.Equals("l")) { LeftInput = 1; }
            bool removing = Dir.Equals("l"); //if turning left

            Pages[2 * (pageNum)].gameObject.SetActive(false);
            Pages[2 * (pageNum) + 1].gameObject.SetActive(false);
            flipping = true;
            PageFlipper.GetComponent<SpriteRenderer>().enabled = true; // show the animation of the page being flipped
            PageFlipper.GetComponent<Animator>().Play("Flip_" + Dir, -1, 0);
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
        if (pageNum >= 0 && pageNum <= 8)
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


    public void Return()
    {
        StartCoroutine(CloseVentory());
    }
    public IEnumerator CloseVentory()
    {
        GetComponent<Animator>().SetTrigger("Close");

        foreach (Transform btn in BookCanvas.GetComponents<Transform>())
            btn.gameObject.SetActive(false);
        foreach (GameObject page in Pages)
            page.SetActive(false);

        yield return new WaitForSeconds(0.32f);

        GM.Instance.Player.GetComponent<Player>().Pausing = false;
        GM.Instance.Player.GetComponent<Player>().rb.gravityScale = 10;
        GM.Instance.Player.GetComponentInChildren<CameraManager>().Pause(false); // camera unpause
        GM.Instance.InVentory = false;
        SceneManager.UnloadSceneAsync(6);
    }

    public void GoToInventory()
    {
        foreach (Transform btn in BookCanvas.GetComponents<Transform>())
            btn.gameObject.SetActive(false);
        foreach (GameObject page in Pages)
            page.SetActive(false);

        Interface.WriteToJsonFile<Save>(Application.persistentDataPath + "/gamesave" + GM.Instance.saveID + ".save", GM.Instance.Save);
        StartCoroutine(GameObject.Find("LevelLoader").GetComponent<LevelLoader>().LoadLevel(0));
    }
}