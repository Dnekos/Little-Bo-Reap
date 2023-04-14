using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class ControlsOptionDisplay : MonoBehaviour
{
    [SerializeField] GameObject mouseMenu;
    [SerializeField] GameObject controllerMenu;

    private void OnEnable()
    {
        if (Gamepad.all.Count > 0)
        {
            controllerMenu.SetActive(true);
            mouseMenu.SetActive(false);
        }
        else
        {
            mouseMenu.SetActive(true);
            controllerMenu.SetActive(false);
        }
    }

}
