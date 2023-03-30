using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMousePosition : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Input.mousePosition + offset * transform.parent.parent.parent.GetComponent<Canvas>().scaleFactor;
    }
}
