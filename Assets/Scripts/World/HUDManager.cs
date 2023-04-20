using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

[System.Serializable]
public struct SheepIcons
{
	public GameObject Marker;
	public Image IconFill;
}

public class HUDManager : MonoBehaviour
{
	[SerializeField]
	PlayerInput inputs;
	[SerializeField] GameObject HUD;

	[Header("Sheep UI")]
	[SerializeField] TextMeshProUGUI redText;
	[SerializeField] TextMeshProUGUI flockNumber;

	[Header("Sheep Swap Variables")]
	[SerializeField] GameObject flockSelectMenu;
	[SerializeField] TextMeshProUGUI[] flockSelectTexts;
	[SerializeField] float flockMenuTimescale = 0.25f;
	[SerializeField] float defaultTimescale = 1;
	[SerializeField] GameObject[] spritePositions;
	[SerializeField] SheepIcons[] sheepIcons;

	[Header("Swap Animation Variables")]
	[SerializeField] Animator SwapUIAnimator;
	[SerializeField] string swapAnimationUI;
	[SerializeField] string noSheepAnimUI;

	[Header("Progression")]
	[SerializeField] GameObject ProgressionMenu;
	public event Action<GameObject> activePanelChange;
	[SerializeField] ProgressionParent[] upgradeTrees;
	[SerializeField] TextMeshProUGUI SoulNumber;
    [SerializeField] Animator SoulUIAnimator;
    [SerializeField] string soulCollectAnimation;
	[SerializeField] FMODUnity.EventReference openMenuSFX;
	[SerializeField] FMODUnity.EventReference closeMenuSFX;

    [Header("Death")]
	[SerializeField] GameObject deathUI;


	public void ToggleHud()
	{
		HUD.SetActive(!HUD.activeInHierarchy);
	}
	public void ToggleHud(bool value)
	{
		HUD.SetActive(value);
	}

	#region Opening and Closing menus
	public void OpenDeathMenu()
	{
		HUD.SetActive(false);
		deathUI.SetActive(true);
	}
	public void CloseDeathMenu()
	{
		HUD.SetActive(true);
		deathUI.SetActive(false);
	}
	public void ToggleProgressionMenu(bool value)
	{
		ProgressionMenu.SetActive(value);

		// pause sounds
		FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay").setPaused(value);
		FMODUnity.RuntimeManager.GetBus("bus:/Music").setPaused(value);

		if (value)
		{
			// mouse 
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			// disable HUD and pause
			ToggleHud(false);
			FMODUnity.RuntimeManager.PlayOneShot(openMenuSFX);
			WorldState.instance.gameState = WorldState.State.Dialog;
			inputs.SwitchCurrentActionMap("Dialog");
			Time.timeScale = 0;
		}
		else
		{
			Time.timeScale = 1;

			// mouse
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			// return to normal gameplay
			ToggleHud(true);
			FMODUnity.RuntimeManager.PlayOneShot(closeMenuSFX);
			WorldState.instance.gameState = WorldState.State.Play;
			inputs.SwitchCurrentActionMap("PlayerMovement");
		}
	}
	#endregion
	private void Start()
	{
		WorldState.instance.HUD = this;
		StartCoroutine(Initialize());
	}

	/// <summary>
	/// needed for set up that may have blockers (other things in start)
	/// </summary>
	private IEnumerator Initialize()
	{
		// wait for two frames
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		for (int i = 0; i < upgradeTrees.Length; i++)
			upgradeTrees[i].CheckLoadedUpgrades();
		UpdateSoulCount(false);
	}
	public void UpdateActiveFlockUI(int currentFlock, string number, Color uiColor)
	{
		SetSheepPositions(currentFlock);

		flockNumber.text = number;
		redText.text = flockNumber.text;
		flockNumber.color = uiColor;
	}

    public void UpdateSoulCount(bool playAnimation = true)
    {
		string currentSouls = Mathf.Min(99, WorldState.instance.PersistentData.soulsCount).ToString();
        SoulNumber.text = currentSouls + " Souls";
		if (playAnimation)
			SoulCollectAnimation();
    }

    private void SetSheepPositions(int currentFlock)
	{
		for(int i = 0; i < sheepIcons.Length; i++)
		{
			int currentPosition = i - currentFlock;

			if(currentPosition < 0)
			{
				currentPosition = sheepIcons.Length + currentPosition;
			}
			else if(currentPosition == 0)
			{
				//activePanelChange.Invoke(sheepIcons[currentFlock].Marker);
			}

			//You can either use the instant move or the lower one which lerps between the points
			//MoveToPosition(spritePositions[currentPosition].transform, sheepIcons[i].Marker.transform);
			LerpUIObject(spritePositions[currentPosition].transform, sheepIcons[i].Marker.transform);
		}
	}

	private void MoveToPosition(Transform Position, Transform Icon)
	{
		Icon.SetParent(Position);
		Icon.localPosition = Vector3.zero;
		Icon.localScale = Vector3.one;
	}

	[SerializeField] float lerpSpeed = 0.05f;

	private IEnumerator LerpingCoroutine(Transform endPosition, Transform uiObject)
	{
		uiObject.SetParent(endPosition);

		Vector3 startPosition = uiObject.localPosition;

		Vector3 startScale = uiObject.localScale;

		float elapsedTime = 0f;
		while (elapsedTime < lerpSpeed)
		{
			float t = elapsedTime / lerpSpeed;
			uiObject.localPosition = Vector3.Lerp(startPosition, Vector3.zero, t);
			uiObject.localScale = Vector3.Lerp(startScale, Vector3.one, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		uiObject.localPosition = Vector3.zero;
		uiObject.localScale = Vector3.one;
	}

	private void LerpUIObject(Transform endPosition, Transform uiObject)
	{
		StartCoroutine(LerpingCoroutine(endPosition, uiObject));
	}

	public void UpdateFlockWheelText(int index, int active, int max)
	{
		flockSelectTexts[index].text = active + "/" + max;
	}

	public void EnableSheepWheel()
	{
		Time.timeScale = flockMenuTimescale;
		Time.fixedDeltaTime = 0.02F * Time.timeScale; //evil physics timescale hack to make it smooth
		flockSelectMenu.gameObject.SetActive(true);
	}
	public void DisableSheepWheel()
	{
		Time.timeScale = defaultTimescale;
		Time.fixedDeltaTime = 0.02F; //evil physics timescale hack
		flockSelectMenu.gameObject.SetActive(false);
	}

	public void SwapAnimation()
	{
		//put code here
	}

	public void SheepErrorAnimation()
	{
		if (SwapUIAnimator.gameObject.activeSelf)
			SwapUIAnimator.Play(noSheepAnimUI);
	}
    public void SoulCollectAnimation()
    {
        if (SoulUIAnimator.gameObject.activeSelf)
            SoulUIAnimator.Play(soulCollectAnimation);
    }
}
	