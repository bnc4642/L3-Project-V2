  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;

    public float transitionTime = 1;

    public void OnEscape()
    {
        StartCoroutine(LoadLevel(0));
    }

    public IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}
