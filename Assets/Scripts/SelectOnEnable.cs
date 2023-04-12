using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnEnable : MonoBehaviour
{
	static Stack<GameObject> focusStack;

	[SerializeField] GameObject UIPiece;
	private void OnEnable()
	{
		if (focusStack == null)
			focusStack = new Stack<GameObject>();
		focusStack.Push(UIPiece);
		EventSystem.current.SetSelectedGameObject(UIPiece);
	}
	private void OnDisable()
	{
		do
		{
			focusStack.Pop();

		} while (focusStack.Count > 0 && (focusStack.Peek() == null || !focusStack.Peek().activeInHierarchy));

		if (focusStack.Count > 0)
			EventSystem.current.SetSelectedGameObject(focusStack.Peek());
	}
}
