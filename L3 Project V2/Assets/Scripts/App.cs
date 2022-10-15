using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    public float i = 0;

    // Runs before a scene gets loaded
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadApp()
    {
        GameObject app = GameObject.Instantiate(Resources.Load("App")) as GameObject;
        GameObject.DontDestroyOnLoad(app);
    }
    // You can choose to add any "Service" component to the Main prefab.
    // Examples are: Input, Saving, Sound, Config, Asset Bundles, Advertisements
}