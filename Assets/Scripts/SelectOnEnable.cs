using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnEnable : MonoBehaviour
{
	[SerializeField] GameObject UIPiece;
	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(UIPiece);

	}
}
