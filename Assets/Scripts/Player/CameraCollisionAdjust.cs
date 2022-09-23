using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionAdjust : MonoBehaviour
{
    [Header("Camera Z Slide Variables")]
    [SerializeField] float zSlideRate;
    [SerializeField] float zSlideMin;
    [SerializeField] float zSlideMax;
    [SerializeField] float zValue;
    [SerializeField] float innerCollideRadius;
    [SerializeField] LayerMask collideLayers;
    Vector3 camPosition;
    public bool isColliding = false;

    private void Start()
    {
        zValue = transform.localPosition.z;
        camPosition = transform.localPosition;
    }

    private void Update()
    {
        if(!isColliding && zValue > zSlideMax)
        {
            zValue += zSlideRate * -Time.deltaTime;
            camPosition.z = zValue;
            transform.localPosition = camPosition;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        isColliding = true;
        if (zValue < zSlideMin && Physics.CheckSphere(transform.position, innerCollideRadius, collideLayers))
        {
            zValue -= zSlideRate * -Time.deltaTime;
            camPosition.z = zValue;
            transform.localPosition = camPosition;
        } 
    }
    private void OnTriggerExit(Collider other)
    {
        isColliding = false;   
    }
}
