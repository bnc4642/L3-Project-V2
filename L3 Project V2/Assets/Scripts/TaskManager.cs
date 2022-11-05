using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public List<Task> Tasks = new List<Task>();
    public GameObject TaskCompletionPrefab;

    public void AddTask(string title, string description) //e.g. "Tim's Lost His Hat", "+ Find Tim's hatl\n+ Return it to him\n+ Reward: $2"
    {
        Task task = new Task(title, description);
    }

    //check for completion of tasks

    public void CompleteTask(int id)
    {
        //show a little <Task Complete!> thing on the side
        GameObject t = Instantiate(TaskCompletionPrefab);
        t.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Tasks[id].Title;
        Destroy(t, 3.5f);
        //update task to show that.

    }
}
