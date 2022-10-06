using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraFollow : MonoBehaviour
{
    [Header("Transform to follow")]
    [SerializeField] Transform followPoint;

	[Header("Camera Variables")]
	[SerializeField] float LerpSpeed = 4;
    [SerializeField] Camera playerCamera;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float xCameraClampMax = 90f;
    [SerializeField] float xCameraClampMin = -90f;

    [Header("Camera Shake")]
    [SerializeField] string bigShakeAnimation;
    [SerializeField] string smallShakeAnimation;

    [Header("Player Orientation")]
    [SerializeField] Transform playerOrientation;
    [SerializeField] Transform playerBody;

                                                                                     
    Vector2 mouseValue; //value taken from input                                     
    float mouseX = 0;
    float mouseY = 0;
    float xRotation = 0f;
    float yRotation = 0f;

    //float zSlideValue = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShakeCamera(bool bigShake)
    {
        if (bigShake) GetComponent<Animator>().Play(bigShakeAnimation);
        else GetComponent<Animator>().Play(smallShakeAnimation);
    }

    private void Update()
    {
        mouseX = mouseValue.x * mouseSensitivity * Time.deltaTime;
        mouseY = mouseValue.y * mouseSensitivity * Time.deltaTime; ;

        //move camera based mouse position
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, xCameraClampMin, xCameraClampMax);
        yRotation += mouseX;

        //move the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

		//move the player orientation transform


	}

	private void LateUpdate()
    {
		playerOrientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        playerOrientation.position = Vector3.Lerp(playerOrientation.position, followPoint.position,Time.deltaTime * LerpSpeed);
        transform.position = Vector3.Lerp(transform.position, followPoint.position, Time.deltaTime * LerpSpeed); 
    }

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        mouseValue = context.ReadValue<Vector2>();
    }

}
