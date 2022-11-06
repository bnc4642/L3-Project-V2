  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelLoader : MonoBehaviour
{
    public Animator Transition;

    public void OnEscape()
    {
        foreach (Enemy E in GameObject.FindObjectsOfType<Enemy>())
        {
            E.Pause();
        }

        //GM.Instance.Player.Pause()

        Debug.Log("Escape");

        StartCoroutine(LoadLevel(0)); //exit levels and just go to inventory scene. Needs some custom code desperately
    }

    public IEnumerator LoadLevel(int levelIndex) //levels are controlled by index within the build settings
    {
        Transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(levelIndex);
    }
}
