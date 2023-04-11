using Newtonsoft.Json.Schema;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickToggle : MonoBehaviour
{
	[SerializeField] Image iconImage;
	[SerializeField] GameObject iconCheck;
	[SerializeField] Color greyedColor;
	[SerializeField] ButtonState buttonState;
	[SerializeField] ButtonType buttonType;
	public StateChangeEvent onStateChanged;

	private void Start()
	{
		SetState(buttonState, false);
	}

	public void SetState(ButtonState buttonState, bool sendMessage)
	{
		this.buttonState = buttonState;

		iconImage.color = Color.white;
		iconCheck.SetActive(false);
		if (buttonState == ButtonState.Locked) iconImage.color = Color.gray;
		if (buttonState == ButtonState.Enabled) iconCheck.SetActive(true);

		if (sendMessage) onStateChanged.Invoke(buttonState, buttonType);
	}

	public void OnButtonClick()
	{
		if (buttonState == ButtonState.Disabled)
		{
			SetState(ButtonState.Enabled, true);
		}
		else
		{
			if (buttonState == ButtonState.Enabled)
			{
				SetState(ButtonState.Disabled, true);
			}
			else
			{
				SetState(ButtonState.Enabled, true);
			}
		}
	}
}
[System.Serializable]
public class ButtonClickToggle
{
	[SerializeField] Image iconImage;
	[SerializeField] GameObject iconCheck;
	[SerializeField] Color greyedColor = Color.gray;
	public Button button;

	public void SetState(ButtonState buttonState)
	{
		iconImage.color = Color.white;
		iconCheck.SetActive(false);
		if (buttonState == ButtonState.Locked) iconImage.color = greyedColor;
		if (buttonState == ButtonState.Enabled) iconCheck.SetActive(true);
	}
}
[System.Serializable]
public class StateChangeEvent : UnityEvent<ButtonState, ButtonType> { }

public enum ButtonState
{
	Locked = -1,
	Disabled = 0,
	Enabled = 1
}

public enum ButtonType
{
	RingingBell,
	Momentum,
	Corruption,
	Cohesion,

	RidingRampage,
	Hardened,
	Sharpness,
	Forcefullness,

	BlackHole,
	Heartiness,
	Stalwart,
	Sustainability
}