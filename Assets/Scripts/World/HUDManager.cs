using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField] GameObject HUD;
    public void ToggleHud()
	{
		HUD.SetActive(!HUD.activeInHierarchy);
	}

	private void Start()
	{
		WorldState.instance.HUD = this;
	}
}
