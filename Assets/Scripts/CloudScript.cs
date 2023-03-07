using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScript : MonoBehaviour
{
    //[SerializeField] float amplitudeX = 10.0f;
    [SerializeField] float amplitudeY = 5.0f;
    //[SerializeField] float xVal = 1.0f;
    [SerializeField] float frequencyY = 5.0f;
    [SerializeField] float timePassed = 0f;
    float xPos;
    float yPos;
    float zPos;

    private void Start()
    {
        xPos = transform.position.x;
        yPos = transform.position.y;
        zPos = transform.position.z;
    }
    public void Update()
    {
        timePassed += Time.deltaTime;
        //float x = amplitudeX * Mathf.Cos(xVal * timePassed);
        float y = Mathf.Abs(amplitudeY * Mathf.Sin(frequencyY * timePassed));
        transform.localPosition = new Vector3(xPos, yPos+y, zPos);
    }
}
