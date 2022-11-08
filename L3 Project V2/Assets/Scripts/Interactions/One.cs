using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class One : Interactable
{
    public override void DialogImpact()
    {
        if (DialogueNums == 0)
        {
            GM.Instance.Save.MapMarker = new float[] { 1.11f, -1.15f };
            GM.Instance.TaskManager.AddTask("Feed The Kids", "Kill the mosquitos and bring back food so that the younger villagers can eat");
        }
        else
        {
            GM.Instance.TaskManager.CompleteTask(0);
        }
        Interface.WriteToJsonFile<Save>(Application.persistentDataPath + "/gamesave" + GM.Instance.saveID + ".save", GM.Instance.Save);
    }

    public override void CheckDialogChanges()
    {
        if (GM.Instance.Save.InventCount[0] > 0 && GM.Instance.Save.InventCount[0] < 6)
            Dialogue[1] = "1 / That won't be enough. They'll need at least 6!";
        else if (GM.Instance.Save.InventCount[0] > 5 && DialogueNums < 2)
        {
            Dialogue.Add("1 / That should be plenty. The kids will eat another day, thanks to you. | 0 / We've gone over this Paul, you don't need to thank me every time. | 2 / Thanks for playing! | 2 / The end!");
            DialogueNums++;
        }
    }
}
