using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Base Interactable")]
    public bool canInteract = true;
    public bool onlyInteractableOnce;

    public virtual void Interact()
    {
        Debug.Log("Interacted with this " + transform.name);

        if (onlyInteractableOnce) canInteract = false;
    }
}
