using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

	public void UpdateActiveFlockUI(Sprite sheepIcon, string number, Color uiColor)
	{
		flockTypeIcon.sprite = sheepIcon;
		flockNumber.text = number;
		redText.text = flockNumber.text;
		flockNumber.color = uiColor;
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
