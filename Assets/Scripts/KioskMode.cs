using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KioskMode : MonoBehaviour
{
    [SerializeField] float timeBeforeCamSpin = 10f;
    [SerializeField] float timeToHideUI = 60f;
    [SerializeField] GameObject UI;
    [SerializeField] float camSpinRate = 10f;
    float currentTime;
    bool uiHidden;
    bool isSpinning = false;
    float yRot = 0;


    private void Start()
    {
        StartCoroutine(StartSpin());
    }

    private void Update()
    {
        if(isSpinning)
        {
            yRot += (camSpinRate * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, yRot, transform.localRotation.z);
        }

        //kiosk mode crap
        currentTime += Time.deltaTime;
        if(currentTime >= timeToHideUI && !uiHidden)
        {
            uiHidden = true;
            UI.SetActive(false);
        }


        if ((Keyboard.current != null && Keyboard.current.anyKey.isPressed) || (Gamepad.current != null && Gamepad.current.leftStick.IsActuated()))
        {
            if(uiHidden)
            {
                uiHidden = false;
                UI.SetActive(true);
            }
            currentTime = 0f;
        }
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        //Debug.Log("onmousemove");

        if(context.performed)
        {
            currentTime = 0f;
            if (uiHidden)
            {
                uiHidden = false;
                UI.SetActive(true);
            }
        }
   
    }

    IEnumerator StartSpin()
    {
        yield return new WaitForSeconds(timeBeforeCamSpin);
        isSpinning = true;
    }
}
