using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public string Identifier;
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetComponent<Animator>().Play(Identifier, -1, 0);
            string[] parts = Identifier.Split('_');
            if (parts[0] == "BookClick")
            {
                StartCoroutine(GetComponentInParent<Inventory>().FlipPage(parts[1]));
            }
        }
    }
}
