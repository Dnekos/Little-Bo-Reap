using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollisionAdjust : MonoBehaviour
{
    [Header("Camera Z Slide Variables")]
    [SerializeField] float slerpRate;
    [SerializeField] float zSlideMax;
	[SerializeField] float collisionOffset = 0.1f;
	[SerializeField] float radius = 0.3f;
    float zValue;
    [SerializeField] LayerMask collideLayers;
    Vector3 camPosition;
    public bool isColliding = false;

    private void Start()
    {
        zValue = transform.localPosition.z;
        camPosition = transform.localPosition;
    }

	
	private void LateUpdate()
	{
		Debug.DrawRay(transform.parent.position, transform.position - transform.parent.position, Color.red);

		RaycastHit info;
		
		Physics.SphereCast(transform.parent.position, radius, - transform.forward, out info, -zSlideMax, collideLayers, QueryTriggerInteraction.Ignore);

		if (info.distance == 0)
			transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, 2,  zValue), Time.deltaTime * slerpRate);
		else
			transform.localPosition = new Vector3(0, 2, collisionOffset - info.distance);


	}
}
