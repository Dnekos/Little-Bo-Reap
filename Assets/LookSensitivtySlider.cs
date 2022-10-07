using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LookSensitivtySlider : MonoBehaviour
{
    [SerializeField] PlayerCameraFollow playerCam;
    [SerializeField] Slider senitivitySlider;
    [SerializeField] TextMeshProUGUI sliderText;

    public void ChangeSensitivity()
    {
        playerCam.mouseSensitivity = senitivitySlider.value * playerCam.mouseSensitivityMax;
        sliderText.text = "Mouse Sensitivity: " + playerCam.mouseSensitivity;
    }
}
