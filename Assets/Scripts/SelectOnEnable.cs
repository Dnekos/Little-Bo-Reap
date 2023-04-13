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

		Debug.Log("pushing " + UIPiece + " from UI Stack");
		focusStack.Push(UIPiece);
		EventSystem.current.SetSelectedGameObject(UIPiece);
	}
	private void OnDisable()
	{
		do
		{
			Debug.Log("popping "+ focusStack.Pop()+" from UI Stack");

		} while (focusStack.Count > 0 && (focusStack.Peek() == null || !focusStack.Peek().activeInHierarchy));

		if (focusStack.Count > 0)
			EventSystem.current.SetSelectedGameObject(focusStack.Peek());
	}
}
