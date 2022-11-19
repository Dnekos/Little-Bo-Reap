using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicCameras : MonoBehaviour
{
	[SerializeField] Camera[] Cameras;
	[SerializeField] Canvas UI;
	int activeCam = 0;
	[SerializeField] GameEvent UIToggle;
	[SerializeField] GameEvent MusicToggle;

	// Start is called before the first frame update
	void Start()
    {
		Cameras[0].enabled = true;
		for (int i = 1; i < Cameras.Length; i++)
			Cameras[i].enabled = false;
	}

	public void OnCamChange(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Cameras[activeCam].enabled = false;
			activeCam = (int)context.ReadValue<float>();
			Debug.Log(activeCam + " " + Cameras[activeCam].gameObject);
			Cameras[activeCam].enabled = true;

			// dunno if this is needed lmao
			//Camera.SetupCurrent(Cameras[activeCam]);
		}
	}

	public void OnToggleUI(InputAction.CallbackContext context)
	{
		UI.enabled = !UI.enabled;
		UIToggle.listener.Invoke();
	}
	public void OnToggleMusic(InputAction.CallbackContext context)
	{
		MusicToggle.listener.Invoke();
	}
}
