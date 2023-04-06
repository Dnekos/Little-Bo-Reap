using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerCameraFollow : MonoBehaviour
{
	[SerializeField] GameEvent ResetAimMode, EndAimMode;

	[Header("Transform to follow")]
    [SerializeField] Transform followPoint;
	[SerializeField] CameraOffsetAdjuster offsetter;
	bool isAiming = false;

	[Header("Camera Variables")]
	[SerializeField] float LerpSpeed = 20;
    [SerializeField] Camera playerCamera;
    public float mouseSensitivityMax = 35f;
    public float mouseSensitivity;
    [SerializeField] float xCameraClampMax = 90f;
    [SerializeField] float xCameraClampMin = -90f;

    [Header("Camera Shake")]
    [SerializeField] float smallShakeMagnitude = 5f;
    [SerializeField] float smallShakeDuration = 1f;
    [SerializeField] float bigShakeMagnitude = 10f;
    [SerializeField] float bigShakeDuration = 1f;

    [Header("Player Orientation")]
    [SerializeField] Transform playerOrientation;
    [SerializeField] Transform playerBody;

    [Header("Kiosk Mode")]
    [SerializeField] float kioskCamSpeed;
    [SerializeField] float timeToTriggerKiosk = 5f;
    [SerializeField] float timeToBootPlayer = 100f;
    [SerializeField] string mainMenu;
    //[SerializeField] GameObject playerHUD;
    bool inKioskMode;
    float currentIdleTime;


	[Header("Aiming")]
	[SerializeField] float duration = 3;
	[SerializeField] Transform OverTheShoulder;
	[SerializeField] float aimFOV = 30;
	float defaultFOV;
	
	// INPUTS
    Vector2 mouseValue; //value taken from input                                     
    float mouseX = 0;
    float mouseY = 0;
    float xRotation = 0f;
    float yRotation = 0f;


    // Start is called before the first frame update
    void Start()
    {
        //lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

		defaultFOV = playerCamera.fieldOfView;

		// default settings
		mouseSensitivity = PlayerPrefs.GetFloat("sensitivity", 1) * mouseSensitivityMax;

		// listners for aiming
		ResetAimMode.Add( () =>
		{
			StopAllCoroutines();
			StartCoroutine(AimTimer());
		});
        EndAimMode.Add(() =>
		{
			StopAllCoroutines();
			isAiming = false;
		});
	}

	#region CameraShake
	public void ShakeCamera(bool bigShake)
    {
        if (bigShake) 
			StartCoroutine(CameraShake(bigShakeMagnitude, bigShakeDuration));
        else 
			StartCoroutine(CameraShake(smallShakeMagnitude, smallShakeDuration));
    }

    IEnumerator CameraShake(float magnitude, float duration)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;

        float elapsed = 0;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, Camera.main.transform.localPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        Camera.main.transform.localPosition = new Vector3(originalPos.x, originalPos.y, Camera.main.transform.localPosition.z);
    }
	#endregion

	private void Update()
    {
        //kiosk mode crap
        if(inKioskMode && ((Keyboard.current != null && Keyboard.current.anyKey.isPressed) || (Gamepad.current != null && Gamepad.current.leftStick.IsActuated())))
        {
            inKioskMode = false;
            WorldState.instance.HUD.ToggleHud(true);
            currentIdleTime = 0f;
        }

		// dont do kiosk stuff while in dialog
		if (WorldState.instance.gameState == WorldState.State.Play)
			currentIdleTime += Time.deltaTime;

        if(currentIdleTime >= timeToTriggerKiosk)
        {
            if (!inKioskMode)
            {
                WorldState.instance.HUD.ToggleHud(false);
                inKioskMode = true;
            }
            yRotation += kioskCamSpeed * Time.deltaTime;

			if (currentIdleTime >= timeToBootPlayer)
			{
				SceneManager.LoadScene(mainMenu);
			}
		}

        mouseX = mouseValue.x * mouseSensitivity * Time.deltaTime;
        mouseY = mouseValue.y * mouseSensitivity * Time.deltaTime;

        //move camera based mouse position
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, xCameraClampMin, xCameraClampMax);
        yRotation += mouseX;

        //move the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
	}

	private void LateUpdate()
	{
		//move the player orientation transform
		playerOrientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);

		playerOrientation.position = Vector3.Lerp(playerOrientation.position, followPoint.position, Time.deltaTime * LerpSpeed);
		if (isAiming)
		{
			transform.position = Vector3.Lerp(transform.position, OverTheShoulder.position, Time.deltaTime * LerpSpeed);
			playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, aimFOV, Time.deltaTime * LerpSpeed);
		}
		else
		{
			transform.position = Vector3.Lerp(transform.position, followPoint.position + offsetter.Offset, Time.deltaTime * LerpSpeed);
			playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * LerpSpeed);
		}
	}

	#region Aim Mode
	IEnumerator AimTimer()
	{
		isAiming = true;
		yield return new WaitForSeconds(duration);
		isAiming = false;
	}
	#endregion

	public void OnMouseLook(InputAction.CallbackContext context)
    {
        if (inKioskMode)
        {
            inKioskMode = false;
			WorldState.instance.HUD.ToggleHud(true);
		}

		mouseValue = context.ReadValue<Vector2>();
        currentIdleTime = 0f;
    }

}
