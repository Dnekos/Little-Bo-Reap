using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class DialogBox : MonoBehaviour
{
	// active dialogline variables
	Speaker currentspeaker; // might not need
	Conversation activeCon;
	int lineIndex = 0;

	// Camerawork
	Camera cinematicCamera;
	Camera gameCamera;

	// text variables
	string Line;
	int textIndex = 0;

	[Header("Timing")]
	[SerializeField]
	float textSpeed = 1;
	float textTimer = 0;
	[SerializeField]
	bool AdvancingText = false;

	[Header("Looking"), SerializeField]
	CameraOffsetAdjuster player;
	[SerializeField] GameEvent EndAimMode;

	[Header("Text"),SerializeField]
	ControlSpriteEvent SpriteChangeEvent;

	[Header("Components")]
	[SerializeField]
	GameObject DialoguePanel;
	[SerializeField]
	TextMeshProUGUI TextBody;
	[SerializeField]
	PlayerInput inputs;
	[SerializeField]
	GameEvent RespawnEvent;

	private void Awake()
	{
		DialoguePanel.SetActive(false); // ensure that the dialogue isn't open when the game loads in
	}
	private void Start()
	{
		gameCamera = Camera.main;
		gameCamera ??= Camera.current;

		cinematicCamera ??= GetComponentInParent<Camera>();
		cinematicCamera.enabled = false;

		inputs ??= WorldState.instance.player.GetComponent<PlayerInput>();

		// register listener for if player skips/dies in dialog
		RespawnEvent.Add(CloseUI);
		SpriteChangeEvent.Add(ChangeSprite);
	}

	/// <summary>
		/// apply defaults to most local variables, turn on AdvancingText
		/// </summary>
	void ResetText()
	{
		textIndex = 0;
		textTimer = 0;
		AdvancingText = true;
		TextBody.text = "";
	}

	#region starting and stopping
	/// <summary>
	/// Turns on the UI gameobjects
	/// </summary>
	public void ActivateUI(Speaker active_conversation)
	{
		// disable HUD
		WorldState.instance.HUD.ToggleHud(false);

		// stop player
		WorldState.instance.player.GetComponent<PlayerMovement>().HaltPlayer();
		EndAimMode.Raise();

		// set up speaker vars
		currentspeaker = active_conversation;
		DialoguePanel.SetActive(true);
		activeCon = currentspeaker.script;
		lineIndex = 0;

		// switch cameras
		gameCamera.enabled = false;
		cinematicCamera.enabled = true;

		WorldState.instance.gameState = WorldState.State.Dialog;
		inputs.SwitchCurrentActionMap("Dialog");

		// player look
		if (player != null)
			player.LookTarget = active_conversation.transform;

		ReadNextLine();
		//ResetText();
	}

	/// <summary>
	/// close the conversation visuals
	/// </summary>
	public void CloseUI()
	{

		// turn off dialog
		TextBody.text = "";
		DialoguePanel.SetActive(false);

		// switch cameras
		gameCamera.enabled = true;
		cinematicCamera.enabled = false;

		// enable HUD and world
		WorldState.instance.HUD.ToggleHud(true);
		if (WorldState.instance.gameState == WorldState.State.Dialog) // if statement needed in case it conflicts with respawn
			WorldState.instance.gameState = WorldState.State.Play;

		// sometimes its not??
		if (inputs.isActiveAndEnabled)
			inputs.SwitchCurrentActionMap("PlayerMovement");

		// player look
		if (player != null)
			player.StopLooking();

	}
	#endregion

	public void OnAdvanceDialog(InputAction.CallbackContext context)
	{
		if (context.performed)
			ReadNextLine();
	}

	public void ReadNextLine()
	{
		// dont continue if not looking at dialogue or its still counting
		if (WorldState.instance.gameState != WorldState.State.Dialog || IsCountingText() || currentspeaker == null) 
			return;
		else if (lineIndex < activeCon.script.Length)
		{
			// change camera pos
			if (activeCon[lineIndex].changeCamera)
			{
				StopAllCoroutines();
				StartCoroutine(ChangeCameraPos(activeCon[lineIndex].CameraPos, Quaternion.Euler(activeCon[lineIndex].CameraEuler), activeCon[lineIndex].CameraTransitionSpeed));
			}
			// set dialogue line
			Line = activeCon[lineIndex++].body;

			// reset variables and move on
			ResetText();
		}
		else // if end of the knot
		{
			DialoguePanel.SetActive(false);
			StartCoroutine(ChangeCameraPos(gameCamera.transform.position, gameCamera.transform.rotation, activeCon.endTime, true));
		}
	}

	IEnumerator ChangeCameraPos(Vector3 pos, Quaternion rot, float speed, bool endCinematic = false)
	{
		Vector3 origPos = cinematicCamera.transform.position;
		Quaternion origRot = cinematicCamera.transform.rotation;

		// slerp the cameras
		if (speed > 0)
		{
			float inverse_time = 1 / speed;
			for (float t = 0; t < 1; t += Time.deltaTime * inverse_time)
			{
				float smoothT = Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t));
				cinematicCamera.transform.position = Vector3.Lerp(origPos, pos, smoothT);
				cinematicCamera.transform.rotation = Quaternion.Lerp(origRot, rot, smoothT);
				yield return new WaitForEndOfFrame();

			}
		}

		// double check that its in the right spot
		cinematicCamera.transform.position = pos;
		cinematicCamera.transform.rotation = rot;
		if (endCinematic)
			CloseUI();
	}


	

	/// <summary>
	/// If still counting out text, fill in the rest of the line and return true.
	/// </summary>
	/// <returns>true if text was still being typed onto the textbox</returns>
	public bool IsCountingText()
	{
		if (AdvancingText) // if still showing dialogue, fully complete dialogue but do not advance
		{
			AdvancingText = false;
			while (textIndex != Line.Length)
				IncrementCurrentLine();
			return true;
		}
		return false;
	}


	// Update is called once per frame
	void Update()
	{
		switch (WorldState.instance.gameState)
		{
			case WorldState.State.Dialog:
				if (currentspeaker == null)
					return;

				textTimer += textSpeed * Time.deltaTime; // increment time
				currentspeaker.SetTalking(AdvancingText);

				if (AdvancingText && textTimer > 1) // incrementing text
				{

					IncrementCurrentLine();

					//SoundManager.PlaySound(Sound.TextScroll); // sound effect

					textTimer = 0; // reset timer

					if (textIndex == Line.Length) // end of phrase
						AdvancingText = false;
				}
				break;
			case WorldState.State.Play:
				cinematicCamera.transform.position = gameCamera.transform.position;
				cinematicCamera.transform.rotation = gameCamera.transform.rotation;
				break;
		}
	}

	void IncrementCurrentLine()
	{
		// make sure we do any TMPro tags in one go
		if (Line[textIndex] == '<')
		{
			string temp = "";
			do
			{
				temp += Line[textIndex++];
			} while (Line[textIndex - 1] != '>' && textIndex < Line.Length);

			TextBody.text += PlayerControlSwitcher.getTextFromAction(temp);
		}
		else
			TextBody.text += Line[textIndex++]; // add next letter and increment
	}

	void ChangeSprite(TMP_SpriteAsset newAsset)
	{
		TextBody.spriteAsset = newAsset;
	}

}
