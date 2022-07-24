using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class Inventory : MonoBehaviour
{
    ArrayList stuff = new ArrayList() { };
    
    public void AddStuff<T>(int index, T thing)
    {
        stuff[index] = thing;
    }

    public void RemoveStuff(int index)
    {
        stuff[index] = null;
    }

    public void Inspect()
    {

    }
}
