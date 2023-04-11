using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicInputDisplay : MonoBehaviour
{
	[Header("Text"), SerializeField]
	ControlSpriteEvent SpriteChangeEvent;
	[SerializeField] float GamePadScale = 2.25f;
	float originalFontSize;

	TextMeshProUGUI keycapText;
	Image KeycapImage;
	string originalText;

    // Start is called before the first frame update
    void Awake()
    {
		keycapText = GetComponentInChildren<TextMeshProUGUI>();
		originalText = keycapText.text;
		KeycapImage = GetComponentInChildren<Image>();
		originalFontSize = keycapText.fontSize;

		SpriteChangeEvent.Add(CheckControls);
	}
	private void Start()
	{
		KeycapImage.enabled = PlayerControlSwitcher._currentController == PlayerControlSwitcher.CurrentControllerType.Keyboard;
		keycapText.text = PlayerControlSwitcher.getTextFromAction(originalText);

	}
	void CheckControls(TMP_SpriteAsset newAsset)
	{
		keycapText.spriteAsset = newAsset;
		KeycapImage.enabled = PlayerControlSwitcher._currentController == PlayerControlSwitcher.CurrentControllerType.Keyboard;
		keycapText.text = PlayerControlSwitcher.getTextFromAction(originalText);
		if (KeycapImage.enabled)
			keycapText.fontSize = originalFontSize;
		else
			keycapText.fontSize = originalFontSize * GamePadScale;

	}
}
