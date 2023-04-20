using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class PlayerControlSwitcher : MonoBehaviour
{
	public enum CurrentControllerType
	{
		Other,
		PlayStation,
		Xbox,
		Keyboard
	}
	static public CurrentControllerType _currentController;

	[SerializeField] ControlSpriteEvent cse;
	[SerializeField] TMPro.TMP_SpriteAsset playstation, xbox;

	private void Start()
	{
		InputSystem.onDeviceChange += InputDeviceChanged;
		CheckCurrentController();
	}

	public void InputDeviceChanged(InputDevice device, InputDeviceChange change)
	{
		CheckCurrentController();
		/*
		switch (change)
		{
			//New device added
			case InputDeviceChange.Added:
				Debug.Log("New device added" + device.description.manufacturer + " name:"+ device.displayName + " interface name:"+ device.description.interfaceName);

				//Checks if is Playstation Controller
				if (device.description.manufacturer == "Sony Interactive Entertainment" && _currentController != CurrentControllerType.PlayStation)
				{
					//Sets UI scheme
					Debug.Log("Playstation Controller Detected");
					//currentImageScheme.SetImagesToPlaystation();
					_currentController = CurrentControllerType.PlayStation;
					//controllerTypeChange.Invoke();
				}
				//Else, assumes Xbox controller
				//device.description.manufacturer for Xbox returns empty string
				else if (device.description.manufacturer != "Sony Interactive Entertainment" && _currentController != CurrentControllerType.Xbox)
				{
					Debug.Log("Xbox Controller Detected");
					//currentImageScheme.SetImagesToXbox();
					_currentController = CurrentControllerType.Xbox;
					//controllerTypeChange.Invoke();
				}
				break;

			//Device disconnected
			case InputDeviceChange.Disconnected:
				//controllerDisconnected.Invoke();
				//_currentController = CurrentControllerType.Other;
				Debug.Log("Device disconnected");
				break;

			//Familiar device connected
			case InputDeviceChange.Reconnected:
				//controllerReconnected.Invoke();
				Debug.Log("Device reconnected");

				//Checks if is Playstation Controller
				if (device.description.manufacturer == "Sony Interactive Entertainment" && _currentController != CurrentControllerType.PlayStation)
				{
					//Sets UI scheme
					Debug.Log("Playstation Controller Detected");
					//currentImageScheme.SetImagesToPlaystation();
					_currentController = CurrentControllerType.PlayStation;
					//controllerTypeChange.Invoke();
				}
				//Else, assumes Xbox controller
				//device.description.manufacturer for Xbox returns empty string
				else if (device.description.manufacturer != "Sony Interactive Entertainment" && _currentController != CurrentControllerType.Xbox)
				{
					Debug.Log("Xbox Controller Detected");
					//currentImageScheme.SetImagesToXbox();
					_currentController = CurrentControllerType.Xbox;
					//controllerTypeChange.Invoke();
				}
				break;

			//Else
			default:
				break;
		}
		*/
	}
	void CheckCurrentController()
	{
		CurrentControllerType oldContType = _currentController;
		if (Gamepad.all.Count > 0)
		{
			if (Gamepad.current is XInputController && _currentController != CurrentControllerType.Xbox)
			{
				// XBOX
				_currentController = CurrentControllerType.Xbox;
				cse.Raise(xbox);
			}
			else if (Gamepad.current is DualShockGamepad && _currentController != CurrentControllerType.PlayStation)
			{
				// PlayStation
				_currentController = CurrentControllerType.PlayStation;
				cse.Raise(playstation);
			}
			else if (!(Gamepad.current is XInputController || Gamepad.current is DualShockGamepad) && _currentController != CurrentControllerType.Other)
			{
				// Other
				_currentController = CurrentControllerType.Other;
			}
		}
		else if (Keyboard.current != null && _currentController != CurrentControllerType.Keyboard)
		{
			// Keyboard
			_currentController = CurrentControllerType.Keyboard;
			cse.Raise(null);
		}

		if (_currentController != oldContType) // if it changed, log it
			Debug.Log("Current Controls: " + _currentController);
	}

	static public string getTextFromAction(string actionname, bool trimBookends = true)
	{
		if (trimBookends)
		{
			char[] charsToTrim = { '<', ' ', '>' };
			actionname = actionname.Trim(charsToTrim);
		}
		PlayerInput pi = WorldState.instance.player.GetComponent<PlayerInput>();
		
		// switch to playermovement action map
		string currentActionMap = pi.currentActionMap.name;
		pi.SwitchCurrentActionMap(pi.defaultActionMap);

		// find action based on name
		InputAction action = pi.actions.FindAction(actionname);
		if (action == null)
		{
			Debug.LogWarning("could not find action: " + actionname);
			pi.SwitchCurrentActionMap(currentActionMap);
			return actionname;
		}

		// check which control to display
		string returner = "";

		if ((_currentController == CurrentControllerType.PlayStation || _currentController == CurrentControllerType.Xbox) && Gamepad.current != null)
		{
			// loop through all bindings
			for (int i = 0; i < action.bindings.Count; i++)
			{
				// check if bindings work on this control scheme (and isnt a composite like the Vector2D used in WASD)
				if (!action.bindings[i].isComposite && InputControlPath.TryFindControl(Gamepad.current, action.bindings[i].effectivePath) != null)
				{
					// make sure to add on the tag to get the TMP_Sprite
					returner += "<sprite name=\"" + InputControlPath.ToHumanReadableString(
						action.bindings[i].effectivePath,
						InputControlPath.HumanReadableStringOptions.OmitDevice) + "\" tint=1> / ";
				}
			}
		}
		else if (_currentController == CurrentControllerType.Keyboard)
		{
			// loop through all bindings
			for (int i = 0; i < action.bindings.Count; i++)
			{
				// check if bindings work on this control scheme (and isnt a composite like the Vector2D used in WASD)
				if (!action.bindings[i].isComposite && InputControlPath.TryFindControl(Keyboard.current, action.bindings[i].effectivePath) != null)
				{
					returner += InputControlPath.ToHumanReadableString(
						action.bindings[i].effectivePath,
						InputControlPath.HumanReadableStringOptions.OmitDevice) + " / ";
				}
				// check for mouse as well
				if (!action.bindings[i].isComposite && InputControlPath.TryFindControl(Mouse.current, action.bindings[i].effectivePath) != null)
				{
					string mouseString = InputControlPath.ToHumanReadableString(
							action.bindings[i].effectivePath,
							InputControlPath.HumanReadableStringOptions.OmitDevice);

					// edge case for scroll wheel
					if (mouseString.CompareTo("Scroll/Up") == 0) // trim off the Up
						mouseString = "Scroll";
					else if (mouseString.CompareTo("Scroll/Down") == 0) // ignore the other side of scrolling
						continue;

					returner += "<sprite name=\"" + mouseString + "\" tint=1> / ";
				}
				if (returner == "W / S / A / D / ")
					returner = "WASD";
			}

		}

		// return to current map
		pi.SwitchCurrentActionMap(currentActionMap);

		// return, with last space cut off
		return returner.TrimEnd(new char[]{ ' ', '/'});
	}

}


