using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public struct SheepIcons
{
	public GameObject Marker;
	public Image IconFill;
}

public class HUDManager : MonoBehaviour
{
	[SerializeField] GameObject HUD;

	[Header("Sheep UI")]
	[SerializeField] TextMeshProUGUI redText;
	[SerializeField] TextMeshProUGUI flockNumber;
	[SerializeField] Image flockTypeIcon;

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



	public void ToggleHud()
	{
		HUD.SetActive(!HUD.activeInHierarchy);
	}
	public void ToggleHud(bool value)
	{
		HUD.SetActive(value);
	}

	private void Start()
	{
		WorldState.instance.HUD = this;
	}

	public void UpdateActiveFlockUI(int currentFlock, string number, Color uiColor)
	{
		//flockTypeIcon.sprite = sheepIcon;
		SetSheepPositions(currentFlock);

		flockNumber.text = number;
		redText.text = flockNumber.text;
		flockNumber.color = uiColor;
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

			MoveToPosition(spritePositions[currentPosition].transform, sheepIcons[i].Marker.transform);
		}
	}

	private void MoveToPosition(Transform Position, Transform Icon)
	{
		Icon.SetParent(Position);
		Icon.localPosition = Vector3.zero;
		Icon.localScale = Vector3.one;
	}

	[SerializeField] float lerpSpeed = 0.5f;

	private void LerpObject(Transform inputTransform, Transform outputTransform, bool usePosition = true, bool useRotation = true, bool useScale = true)
	{
		StartCoroutine(LerpObjectCoroutine(inputTransform, outputTransform, usePosition, useRotation, useScale));
	}

	IEnumerator LerpObjectCoroutine(Transform inputTransform, Transform outputTransform, bool usePosition, bool useRotation, bool useScale)
	{
		bool isLerping = true;

		Vector3 startPosition = inputTransform.position;
		Vector3 startRotation = inputTransform.rotation.eulerAngles;
		Vector3 startScale = inputTransform.localScale;

		Vector3 endPosition = outputTransform.position;
		Vector3 endRotation = outputTransform.rotation.eulerAngles;
		Vector3 endScale = outputTransform.localScale;

		float elapsedTime = 0f;

		while (isLerping)
		{
			if (usePosition)
			{
				transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / lerpSpeed);
			}

			if (useRotation)
			{
				transform.rotation = Quaternion.Euler(Vector3.Lerp(startRotation, endRotation, elapsedTime / lerpSpeed));
			}

			if (useScale)
			{
				transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / lerpSpeed);
			}

			elapsedTime += Time.deltaTime;

			if (elapsedTime >= lerpSpeed)
			{
				isLerping = false;
			}

			yield return null;
		}
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

}
