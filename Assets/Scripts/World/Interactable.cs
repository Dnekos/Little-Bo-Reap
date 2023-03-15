using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Base Interactable")]
    public bool canInteract = true;
    public bool onlyInteractableOnce;

	[Header("Input Canvas"), SerializeField]
	protected Transform inputIcon;
	protected Transform maincam;

	virtual protected void Start()
	{
		if (inputIcon != null)
		{
			inputIcon.gameObject.SetActive(false);
			maincam = Camera.main.transform;
		}
	}

	public virtual void Interact()
    {
        Debug.Log("Interacted with this " + transform.name);

        if (onlyInteractableOnce)
			canInteract = false;
    }

	/// <summary>
	/// A second Interact function, to be used for things that dont need player feedback, like playing single use interactions when loading
	/// </summary>
	public virtual void InteractBackend()
	{
		if (onlyInteractableOnce)
			canInteract = false;
	}


	virtual protected void Update()
	{
		if (inputIcon != null)
			inputIcon.rotation = maincam.rotation;
	}

	virtual protected void OnTriggerEnter(Collider other)
	{
		// if entered on the forced
		if (other.gameObject == WorldState.instance.player && inputIcon != null && canInteract)
			inputIcon.gameObject.SetActive(true);

	}
	virtual protected void OnTriggerExit(Collider other)
	{
		if (other.gameObject == WorldState.instance.player && inputIcon != null && canInteract)
			inputIcon.gameObject.SetActive(false);
	}
}
