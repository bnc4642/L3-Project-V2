using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    public static GM Instance { get; private set; }

    public int transitionID = 0;

    public GameObject playerPref;

    // level data, gained from LevelData.cs
    public List<float> xToY = new List<float>(); // 4 floats, from xMin to yMin to xMax to yMax

    public List<Vector3> UpTrans = new List<Vector3>(); //transitions between levels
    public List<Vector3> DownTrans = new List<Vector3>();
    public List<Vector3> LeftTrans = new List<Vector3>();
    public List<Vector3> RightTrans = new List<Vector3>();

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; // this is a singleton, so only one can exist in the scene
        }
    }

    public void SpawnPlayer(List<Enemy> Enemies)
    {
        Vector3 position = new Vector3();

        //check for id through all transition locations
        switch (transitionID)
        {
            case < -20:
                foreach (Vector3 pos in UpTrans)
                {
                    if (transitionID == pos.z)
                    {
                        position = pos;
                        break;
                    }
                }
                break;
            case < 0:
                foreach (Vector3 pos in RightTrans)
                {
                    if (transitionID == pos.z)
                    {
                        position = pos;
                        break;
                    }
                }
                break;
            case > 20:
                foreach (Vector3 pos in DownTrans)
                {
                    if (transitionID == pos.z)
                    {
                        position = pos;
                        break;
                    }
                }
                break;
            case >= 0:
                foreach (Vector3 pos in LeftTrans)
                {
                    if (transitionID == pos.z)
                    {
                        position = pos;
                        break;
                    }
                }
                break;
            default:
                break;
        }
        if (position != Vector3.zero)
        {
            //spawn player
            GameObject a = Instantiate(playerPref, position, Quaternion.Euler(0, 0, 0));
            a.GetComponent<Player>().SetLocalVariables();

            //give references
            foreach (Enemy enemy in Enemies)
                enemy.plyr = a.GetComponent<Player>();

            //don't let player just fall back through bottom doors
            if (transitionID > 20)
                a.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 20);
        }
    }
}
