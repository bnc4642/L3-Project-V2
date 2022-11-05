using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TxtBox : MonoBehaviour
{
    public Text text;
    private RectTransform parentRect;
    [SerializeField]
    private float longestCharWidth = 1;
    bool callBack = false;

    public int ID;
    private void Start()
    {
        //subscribe to events
        GameEvents.current.onTxtBoxSelect += TxtBoxSelect;
        GameEvents.current.onTxtBoxDeSelect += TxtBoxDeselect;
        GameEvents.current.onSetTxtBoxValue += SetTxtBoxValue;

        parentRect = GetComponent<RectTransform>();
    }

    //results of events
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

    //custom overflow

    public void UpdateText()
    {
        if (!callBack && this.isActiveAndEnabled) // Needed to be split because events won't call IEnumerators
            StartCoroutine(WaitForUpdate());
        else
            callBack = false; // When hte text is edited in this code, not by the user, the function is run again resulting in a feedback loop
    }

    private IEnumerator WaitForUpdate()
    {
        yield return new WaitForSeconds(0.03f); // This code is called before the text property is updated, so it needs to wait
        text.text = transform.GetChild(0).GetComponentsInChildren<TMPro.TextMeshProUGUI>()[1].text;

        if (TextTooLong())
            text.text = text.text.Substring(0, text.text.Length - 2); // Removes the last character on the text
        else
            callBack = true;

        SetTxtBoxValue(ID, text.text); // Required to access properties text.rectTransform
    }

    private bool TextTooLong()
    {
        float textWidth = LayoutUtility.GetPreferredWidth(text.rectTransform); //the width the text would LIKE to be
        float parentWidth = parentRect.rect.width; //the actual width of the text's parent container
        return (textWidth > (parentWidth - longestCharWidth)); //if the text almost too wide, stop when the next character could be too wide
    }
}
