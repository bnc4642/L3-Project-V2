using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class App : MonoBehaviour
{
    // Runs before the first scene gets loaded
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadApp()
    {
        GameObject app = Instantiate(Resources.Load("App")) as GameObject;
        DontDestroyOnLoad(app);
    }
    // Componenets like transition manager, or settings or game manager are added to this prefab
}