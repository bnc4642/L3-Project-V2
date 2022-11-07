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

        GM.Instance.transitionID = GM.Instance.Save.SavePoint[1];

        StartCoroutine(LoadLevel(GM.Instance.Save.SavePoint[0])); //exit inventory scene and enter level at last save point. Needs some custom code desperately
    }

    public IEnumerator LoadLevel(int levelIndex) //levels are controlled by index within the build settings
    {
        if (levelIndex != 6)
        {
            GM.Instance.SaveLocation(levelIndex);

            Transition.SetTrigger("Start");

            yield return new WaitForSeconds(1);

            SceneManager.LoadScene(levelIndex);
        }
        else
            SceneManager.LoadScene(levelIndex, LoadSceneMode.Additive);
    }
}
