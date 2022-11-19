using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientWorldSpaceCanvasToCamera : MonoBehaviour
{
    void Update()
    {
		if (Camera.main != null && Camera.main.enabled)
			transform.LookAt(Camera.main.transform);
    }
}
