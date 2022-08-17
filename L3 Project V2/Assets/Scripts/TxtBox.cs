using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TxtBox : MonoBehaviour
{
    public int ID;
    private void Start()
    {
        GameEvents.current.onTxtBoxSelect += TxtBoxSelect;
        GameEvents.current.onTxtBoxDeSelect += TxtBoxDeselect;
        GameEvents.current.onSetTxtBoxValue += SetTxtBoxValue;
    }

    public void TxtBoxSelect(int id)
    {
        if (ID == id)
            GetComponent<TMPro.TMP_InputField>().ActivateInputField();
    }
    public void TxtBoxDeselect(int id)
    {
        if (ID == id)
            GetComponent<TMPro.TMP_InputField>().DeactivateInputField();
    }
    public void SetTxtBoxValue(int id, string value)
    {
        if (ID == id)
            GetComponent<TMPro.TMP_InputField>().text = value;
    }
}
