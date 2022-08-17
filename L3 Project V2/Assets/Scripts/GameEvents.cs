using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action<int> onTxtBoxSelect;
    public event Action<int> onTxtBoxDeSelect;
    public event Action<string> onReturnTxtBoxValue;
    public event Action<int, string> onSetTxtBoxValue;
    public void TxtBoxSelect(int id)
    {
        if (onTxtBoxSelect != null)
        {
            onTxtBoxSelect(id);
        }
    }
    public void TxtBoxDeselect(int id)
    {
        if (onTxtBoxDeSelect != null)
        {
            onTxtBoxDeSelect(id);
        }
    }

    public void ReturnTxtBoxValue(string value)
    {
        if (onReturnTxtBoxValue != null)
        {
            onReturnTxtBoxValue(value);
        }
    }
    public void SetTxtBoxValue(int id, string value)
    {
        if (onSetTxtBoxValue != null)
        {
            onSetTxtBoxValue(id, value);
        }
    }

}
