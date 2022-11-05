using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Save
{
    //need to figure out which variables are saved in here properly.

    public string Name = "";

    // public int InkpotNo = 0;
    public int Health = 0;
    public string[,] Skills = new string[6, 2] { { "slash", "1" }, { "dash", "0" }, { "heal", "0" }, { "cling", "0" }, { "slam", "0" }, { "float", "0" } };

    // public List<int> MapUnlocked = new List<int>(); 
    // public List<int> CompletedCutscenes = new List<int>(); 
    //public List<int> BossesUndefeated = new List<int>();

    public string mapList = "1";
    public int EncounterProgress = 0;
    public ArrayList Inventory = new ArrayList() { };

    public Save()
    {

    }


    public void AddStuff<T>(int index, T thing)
    {
        Inventory[index] = thing;
    }

    public void RemoveStuff(int index) //called in special interactions
    {
        Inventory[index] = null;
    }
}
