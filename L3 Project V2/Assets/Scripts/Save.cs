using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Save
{
    //public List<int> BossesUndefeated = new List<int>();

    public string Name = "";

    public string[,] Skills = new string[6, 2] { { "slash", "1" }, { "dash", "0" }, { "heal", "0" }, { "cling", "0" }, { "slam", "0" }, { "float", "0" } };

    public int[] SavePoint = new int[2] { 2, 0 }; //scene ID, transition ID

    public List<int> MinorInteractions = new List<int>() { 0, 0 };

    public string MajorInteractions = "";

    public string mapList = "1";
    public List<string> Items = new List<string>() { "food", "drink", "coins", "etc", "etc", "etc" };
    public List<int> InventCount = new List<int>() { 0, 0, 0, 0, 0, 0 };

    public List<Task> Tasks = new List<Task>();

    public float[] MapMarker = new float[2] { 0, 0 };

    public Save()
    {

    }
}
