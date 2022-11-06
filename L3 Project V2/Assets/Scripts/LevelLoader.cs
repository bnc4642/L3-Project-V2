  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelLoader : MonoBehaviour
{
    public Animator Transition;

    public void EnterGame()
    {
        Interface interf = GameObject.FindObjectOfType<Interface>();

        foreach (Transform btn in interf.BookCanvas.GetComponentsInChildren<Transform>())
            btn.gameObject.SetActive(false);

        Save s = Interface.ReadFromJsonFile<Save>(Application.persistentDataPath + "/gamesave" + GM.Instance.saveID + ".save");

        StartCoroutine(LoadLevel(0)); //exit levels and just go to inventory scene. Needs some custom code desperately
    }

    public IEnumerator LoadLevel(int levelIndex) //levels are controlled by index within the build settings
    {
        Transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(levelIndex);
    }
}
