using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;

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
	bool closing = false;
	Coroutine camSwitch;

	[Header("Panel Animation"), SerializeField]
	float EnterPos = 0;
	[SerializeField] float ExitPos = -406, animTime = 0.5f;


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

	[SerializeField] bool isFinalConvo = false;
	bool isFadeDelay = false;
	bool clawIsIn = false;

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
	public void ActivateUI(Speaker active_conversation, bool isFinalConversation)
	{
		closing = false;

        StopAllCoroutines();

		// disable HUD
		WorldState.instance.HUD.ToggleHud(false);

		// stop player
		WorldState.instance.player.GetComponent<PlayerMovement>().HaltPlayer();
		EndAimMode.Raise();

		if (isFinalConversation)
		{
            StartCoroutine(FadeDelay(active_conversation));
        }
		else
		{
            currentspeaker = active_conversation;
            StartCoroutine(MoveTextBox(true));

            if (currentspeaker.scriptIndex == 0)
                activeCon = currentspeaker.script;
            else
                activeCon = currentspeaker.repeatingScript;

            lineIndex = 0;

            // switch cameras
            gameCamera.enabled = false;
            cinematicCamera.enabled = true;

            WorldState.instance.gameState = WorldState.State.Dialog;
            inputs.SwitchCurrentActionMap("Dialog");

			WorldState.instance.HUD.ToggleHud(false);

			// player look
			if (player != null)
                player.LookTarget = active_conversation.transform;

            ReadNextLine();
            //ResetText();
        }
	}

	/// <summary>
	/// close the conversation visuals
	/// </summary>
	public void CloseUI()
	{
		Debug.Log("closeUI");
	//	inputs.DeactivateInput();

		if (isFinalConvo == false) //runs normal end script if this isn't the final dialogue
		{
            // turn off dialog
            StartCoroutine(MoveTextBox(false));
            closing = true;

            // player look
            if (player != null)
                player.StopLooking();
        }
		else //if this is the final dialogue, loops through until THE CLAW is fully animated.
		{ 
			if (clawIsIn == true) //this is set to be true by THE CLAW itself after it's done animating.
			{
                // turn off dialog
                StartCoroutine(MoveTextBox(false));
                closing = true;

                // player look
                if (player != null)
                    player.StopLooking();

                inputs.ActivateInput();

                // save game
                if (WorldState.instance != null)
                {
                    WorldState.instance.SetSaveNextLevel(SceneManager.GetSceneByName("Main_Menu").buildIndex);
                }

                FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
                myBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
                SceneManager.LoadScene("Main_Menu");
            }
			else //when THE CLAW is not active, then make it active.
			{
				if (WorldState.instance.player.transform.parent.gameObject.transform.GetChild(7).GetChild(1).gameObject.activeInHierarchy == false)
				{
                    WorldState.instance.player.transform.parent.gameObject.transform.GetChild(7).GetChild(1).gameObject.SetActive(true); //ACTIVATE THE CLAW!!!!
                }
            }
			
        }

	}

	IEnumerator MoveTextBox(bool entering)
	{
		Debug.Log("startMove");
		RectTransform rt = DialoguePanel.GetComponent<RectTransform>();

		float rate = 1 / animTime;
		if (entering)
		{
			DialoguePanel.SetActive(true);

			rt.anchoredPosition = new Vector2(0, ExitPos);
			for (float t = 0; t < 1; t += Time.unscaledDeltaTime * rate)
			{
				float smoothT = Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t));

				rt.anchoredPosition = new Vector2(0, Mathf.Lerp(ExitPos, EnterPos, smoothT));
				//t += animTime * Time.unscaledDeltaTime;
				//Debug.Log(t + " " + rt.anchoredPosition);

				yield return new WaitForEndOfFrame();
			}
			rt.anchoredPosition = new Vector2(0, EnterPos);

			// switch cameras
			gameCamera.enabled = false;
			cinematicCamera.enabled = true;

		}
		else
		{
			rt.anchoredPosition = new Vector2(0, EnterPos);
			for (float t = 0; t < 1; t += Time.unscaledDeltaTime * rate)
			{
				float smoothT = Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, t));

				rt.anchoredPosition = new Vector2(0, Mathf.Lerp(EnterPos, ExitPos, smoothT));
				//t += animTime * Time.unscaledDeltaTime;
				//Debug.Log(t + " " + rt.anchoredPosition);
				yield return new WaitForEndOfFrame();
			}
			rt.anchoredPosition = new Vector2(0, ExitPos);
			DialoguePanel.SetActive(false);

			// switch cameras
			gameCamera.enabled = true;
			cinematicCamera.enabled = false;

			// enable HUD and world
			WorldState.instance.HUD.ToggleHud(true);
			if (WorldState.instance.gameState == WorldState.State.Dialog) // if statement needed in case it conflicts with respawn
				WorldState.instance.gameState = WorldState.State.Play;

			// turn off dialog
			TextBody.text = "";

			// sometimes its not??
			inputs.ActivateInput();
			if (inputs.isActiveAndEnabled)
				inputs.SwitchCurrentActionMap("PlayerMovement");

			closing = false;
		}


	}
	#endregion

	public void OnAdvanceDialog(InputAction.CallbackContext context)
	{
		if (context.performed)
			ReadNextLine();
	}

	public void ReadNextLine()
	{
		if (isFadeDelay == false) //only progress if this is not the 'final convo'
		{
            // dont continue if not looking at dialogue or its still counting
            if (WorldState.instance.gameState != WorldState.State.Dialog || IsCountingText() || currentspeaker == null)
                return;
            else if (lineIndex < activeCon.script.Length)
            {
                // change camera pos
                if (activeCon[lineIndex].changeCamera)
                {
                    if (camSwitch != null)
                        StopCoroutine(camSwitch);
                    camSwitch = StartCoroutine(ChangeCameraPos(activeCon[lineIndex].CameraPos, Quaternion.Euler(activeCon[lineIndex].CameraEuler), activeCon[lineIndex].CameraTransitionSpeed));
                }
                // set dialogue line
                Line = activeCon[lineIndex++].body;

                // reset variables and move on
                ResetText();
            }
            else // if end of the knot
            {
                //DialoguePanel.SetActive(false);
                StartCoroutine(ChangeCameraPos(gameCamera.transform.position, gameCamera.transform.rotation, activeCon.endTime, true));
            }
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
		if (endCinematic && !closing)
			CloseUI();

		camSwitch = null;
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
					// band-aid, close HUD
					WorldState.instance.HUD.ToggleHud(false);

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

	public void SetFinalDialogue(bool setting) //hacky and stupid way to make the final ending work
	{
		isFinalConvo = setting;
	}

    public void SetClawOpen(bool setting) //hacky and stupid way to make the final ending work
    {
        clawIsIn = setting;
    }
    IEnumerator FadeDelay(Speaker active_conversation)
	{
        isFadeDelay = true;

        // set up speaker vars
        currentspeaker = active_conversation;

        if (currentspeaker.scriptIndex == 0)
            activeCon = currentspeaker.script;
        else
            activeCon = currentspeaker.repeatingScript;

        lineIndex = 0;

        // switch cameras
        gameCamera.enabled = false;
        cinematicCamera.enabled = true;

        WorldState.instance.gameState = WorldState.State.Dialog;
        inputs.SwitchCurrentActionMap("Dialog");

        // player look
        if (player != null)
            player.LookTarget = active_conversation.transform;

        yield return new WaitForSeconds(6);

		isFadeDelay = false;

        StartCoroutine(MoveTextBox(true));

        ReadNextLine();
        //ResetText();
    }
}
