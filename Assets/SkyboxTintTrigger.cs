using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxTintTrigger : MonoBehaviour
{
    [SerializeField] float newExposure;
    [SerializeField] float oldExposure;
    [SerializeField] Color newColor;
    [SerializeField] Color oldColor;
    [SerializeField] float lerpDuration;
    float timeElapsed;
    bool hasTriggered = false;
    bool isDone = false;

    private void Start()
    {
        RenderSettings.skybox.SetColor("_Tint", oldColor);
        RenderSettings.skybox.SetFloat("_Exposure", oldExposure);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            //oldColor = RenderSettings.skybox.color;
        }
    }

    private void Update()
    {
        if(hasTriggered && timeElapsed < lerpDuration)
        {
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(oldColor, newColor, timeElapsed / lerpDuration));
            RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(oldExposure, newExposure, timeElapsed / lerpDuration));

            timeElapsed += Time.deltaTime;
        }
    }
}
