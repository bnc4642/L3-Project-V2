using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    public static GM Instance { get; private set; }

    public TaskManager TaskManager;

    public int transitionID = 0;

    public int saveID = 0;


    public GameObject playerPref;

    public Save Save;
    public GameObject Player;

    // level data, gained from LevelData.cs
    public List<float> xToY = new List<float>(); // 4 floats, from xMin to yMin to xMax to yMax

    public List<Vector3> UpTrans = new List<Vector3>(); //transitions between levels
    public List<Vector3> DownTrans = new List<Vector3>();
    public List<Vector3> LeftTrans = new List<Vector3>();
    public List<Vector3> RightTrans = new List<Vector3>();

    public int health = 5;
    public int energyLevel = 0;

    public bool InVentory = false;

    public bool Died;

    void Awake()
    {
        Save = Interface.ReadFromJsonFile<Save>(Application.persistentDataPath + "/gamesave" + 0 + ".save");

        if (Instance == null)
        {
            Instance = this; // this is a singleton, so only one can exist in the scene
        }
    }

    public void SpawnPlayer(List<Enemy> Enemies)
    {
        if (Save.MajorInteractions == "")
        {
            Player = Instantiate(playerPref, new Vector2(7.1f, -5.45f), Quaternion.Euler(0, 0, 0));
            Player.GetComponent<Player>().SetLocalVariables();
            Player.GetComponent<Player>().SetStates(5, 0);
            Player.GetComponent<Player>().FirstTransition = false;
            Player.GetComponent<Player>().Interactable = FindObjectOfType<Interactable>();
            StartCoroutine(Player.GetComponent<Player>().TotalPostponer(false));
            Save.MajorInteractions = "1";
            return;
        }

        Vector3 position = new Vector3();
        bool foundIt = false;

        //check for id through all transition locations

        if (transitionID == 0)
        {
            List<Vector3>[] fullList = { UpTrans, DownTrans, LeftTrans, RightTrans };

            foreach (List<Vector3> collection in fullList)
            {
                foreach (Vector3 vect in collection)
                {
                    position = vect;
                    break;
                }
            }

        }

        if (!foundIt)
        {
            foreach (Vector3 pos in UpTrans)
            {
                if (transitionID == pos.z)
                {
                    position = pos;
                    foundIt = true;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (Vector3 pos in DownTrans)
            {
                if (transitionID == pos.z)
                {
                    position = pos;
                    foundIt = true;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (Vector3 pos in LeftTrans)
            {
                if (transitionID == pos.z)
                {
                    position = pos;
                    foundIt = true;
                    break;
                }
            }
        }
        if (!foundIt)
        {
            foreach (Vector3 pos in RightTrans)
            {
                if (transitionID == pos.z)
                {
                    position = pos;
                    foundIt = true;
                    break;
                }
            }
        }


        if (position != Vector3.zero)
        {
            //spawn player
            Player = Instantiate(playerPref, position, Quaternion.Euler(0, 0, 0));
            Player.GetComponent<Player>().SetLocalVariables();
            Player.GetComponent<Player>().SetStates(health, energyLevel);
            //don't let player just fall back through bottom doors
        }

        Interface.WriteToJsonFile<Save>(Application.persistentDataPath + "/gamesave" + saveID + ".save", Save);
    }

    public void AddToMap(int id)
    {
        if (!Save.mapList.Contains(id.ToString()))
            Save.mapList += id;
        Interface.WriteToJsonFile(Application.persistentDataPath + "/gamesave" + saveID + ".save", Save);
    }

    public void SaveLocation(int index)
    {
        Save.SavePoint = new int[] { index, transitionID };
    }
}
