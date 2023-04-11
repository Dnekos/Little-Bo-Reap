using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMousePosition : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    public bool followMouse = false;
    // Start is called before the first frame update
    void Start()
    {
        if(Gamepad.all.Count > 0)
        {
            Debug.Log("gamepad detected!");
            followMouse = false;
        }
        else
        {
            Debug.Log("no gamepad?");
            followMouse = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(followMouse)
        {
            this.transform.position = Input.mousePosition + offset * transform.parent.parent.parent.GetComponent<Canvas>().scaleFactor;
        }
        
    }
}
