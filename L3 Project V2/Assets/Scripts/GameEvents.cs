using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        current = this; //it's a singleton, so only one (current) can be present in the scene
    }

    //events
    public event Action<int> onTxtBoxSelect;
    public event Action<int> onTxtBoxDeSelect;
    public event Action<string> onReturnTxtBoxValue;
    public event Action<int, string> onSetTxtBoxValue;

    //event methods
    public void TxtBoxSelect(int id)
    {
        if (onTxtBoxSelect != null)
        {
            onTxtBoxSelect(id); //call event to subscribers
        }
    }
    public void TxtBoxDeselect(int id)
    {
        if (onTxtBoxDeSelect != null)
        {
            onTxtBoxDeSelect(id); //call event to subscribers
        }
    }
    public void ReturnTxtBoxValue(string value)
    {
        if (onReturnTxtBoxValue != null)
        {
            onReturnTxtBoxValue(value); //call event to subscribers
        }
    }
    public void SetTxtBoxValue(int id, string value)
    {
        if (onSetTxtBoxValue != null)
        {
            onSetTxtBoxValue(id, value); //call event to subscribers
        }
    }

}
