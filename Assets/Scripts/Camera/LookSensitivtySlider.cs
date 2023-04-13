using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LookSensitivtySlider : MonoBehaviour
{
    [SerializeField] PlayerCameraFollow playerCam;
	[SerializeField] Slider senitivitySlider;
	[SerializeField] Slider musicSlider;
	[SerializeField] Slider SFXSlider;
	[SerializeField] TextMeshProUGUI sliderText;

	private void Start()
	{
		senitivitySlider.value = PlayerPrefs.GetFloat("sensitivity", senitivitySlider.value);
		musicSlider.value = PlayerPrefs.GetFloat("music", 1) * 100;
		SFXSlider.value = PlayerPrefs.GetFloat("sfx", 1) * 100;
	}

	public void ChangeSensitivity(float value)
    {
        playerCam.mouseSensitivity = value * playerCam.mouseSensitivityMax;
       // sliderText.text = "Mouse Sensitivity: " + (int)playerCam.mouseSensitivity;
		PlayerPrefs.SetFloat("sensitivity", value);
	}
	public void ChangeMusicVolume(float value)
	{
		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
		myBus.setVolume(value * 0.01f);
		PlayerPrefs.SetFloat("music", value * 0.01f);
	}
	public void ChangeSFXVolume(float value)
	{
		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		myBus.setVolume(value * 0.01f);
		PlayerPrefs.SetFloat("sfx", value * 0.01f);
	}
}
