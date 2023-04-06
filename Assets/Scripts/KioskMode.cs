using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KioskMode : MonoBehaviour
{
    [SerializeField] float timeBeforeCamSpin = 10f;
    [SerializeField] float camSpinRate = 10f;
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
    }

    IEnumerator StartSpin()
    {
        yield return new WaitForSeconds(timeBeforeCamSpin);
        isSpinning = true;
    }
}
