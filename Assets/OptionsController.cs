using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class OptionsController : MonoBehaviour
{
	[SerializeField] GameObject Tab1;
	[SerializeField] GameObject Options;
	[SerializeField] GameObject Tab2;
	[SerializeField] GameObject Controls;
	[SerializeField] GameObject Tab3;
	[SerializeField] GameObject Credits;

	[SerializeField] GameObject SelectedGameObject;

	[SerializeField] GameObject currentTab;

	void OnEnable()
	{
		currentTab = Tab1;
		SelectedGameObject.transform.SetParent(currentTab.transform);
		SelectedGameObject.transform.localPosition = new Vector2(0, 0);

		Options.SetActive(true);
		Controls.SetActive(false);
		Credits.SetActive(false);
	}

	public void OnTabLeft(InputAction.CallbackContext context)
	{
		if (context.performed)
			NavigateToPreviousTab();
	}
	public void OnTabRight(InputAction.CallbackContext context)
	{
		if (context.performed)
			NavigateToNextTab();
	}

	private void NavigateToNextTab()
	{
		if (!gameObject.activeInHierarchy)
			return;
		if (currentTab == Tab1)
		{
			currentTab = Tab2;
		}
		else if (currentTab == Tab2)
		{
			currentTab = Tab3;
		}
		else if (currentTab == Tab3)
		{
			currentTab = Tab1;
		}

		SwitchToTab(currentTab);
	}

	private void NavigateToPreviousTab()
	{
		if (!gameObject.activeInHierarchy)
			return;

		if (currentTab == Tab1)
		{
			currentTab = Tab3;
		}
		else if (currentTab == Tab2)
		{
			currentTab = Tab1;
		}
		else if (currentTab == Tab3)
		{
			currentTab = Tab2;
		}

		SwitchToTab(currentTab);
	}

	private void SwitchToTab(GameObject tab)
	{
		Options.SetActive(false);
		Controls.SetActive(false);
		Credits.SetActive(false);

		if (tab == Tab1) Options.SetActive(true);
		else if (tab == Tab2) Controls.SetActive(true);
		else if (tab == Tab3) Credits.SetActive(true);

		SelectedGameObject.transform.SetParent(tab.transform);
		SelectedGameObject.transform.localPosition = new Vector2(0, 0);
	}
}
