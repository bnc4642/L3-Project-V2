using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public GameObject TaskCompletionPrefab;
    public GameObject NewTaskPrefab;

    public void AddTask(string title, string description) //e.g. "Tim's Lost His Hat", "+ Find Tim's hatl\n+ Return it to him\n+ Reward: $2"
    {
        GM.Instance.Save.Tasks.Add(new Task(title, description));

        GameObject t = Instantiate(NewTaskPrefab);
        t.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = title;
        Destroy(t, 3.5f);
    }

    //check for completion of tasks

    public void CompleteTask(int id)
    {
        //show a little <Task Complete!> thing on the side
        GameObject t = Instantiate(TaskCompletionPrefab);
        t.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = GM.Instance.Save.Tasks[id].Title;
        Destroy(t, 3.5f);
        //update task to show that.

    }
}
