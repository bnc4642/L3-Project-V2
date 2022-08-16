using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public string Identifier;
    private bool mouseOver = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (mouseOver)
        {
            GetComponent<Animator>().Play(Identifier, -1, 0);
            string[] parts = Identifier.Split('_');
            if (parts[0] == "BookClick")
            {
                StartCoroutine(GetComponentInParent<Interface>().FlipPage(parts[1]));
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }
}
