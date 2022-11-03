using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour
{
    //variables
    public List<float> xMinToYMax = new List<float>(4); // xMin, xMax, yMin, yMax

    public List<Vector3> UpTransistors = new List<Vector3>();
    public List<Vector3> DownTransistors = new List<Vector3>();
    public List<Vector3> LeftTransistors = new List<Vector3>();
    public List<Vector3> RightTransistors = new List<Vector3>();
    // check transID from all lists to find the right one, check which direction it's in, and then instant player with correct velocity.

    public List<Enemy> Enemies = new List<Enemy>();

    private void Awake() //set up the global controller, and this way there's no need for camera or player references
    {
        GM.Instance.xToY = xMinToYMax;

        GM.Instance.UpTrans = UpTransistors;
        GM.Instance.DownTrans = DownTransistors;
        GM.Instance.LeftTrans = LeftTransistors;
        GM.Instance.RightTrans = RightTransistors;

        GM.Instance.SpawnPlayer(Enemies);
    }
}
