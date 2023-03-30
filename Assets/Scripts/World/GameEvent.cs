using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "ScriptableObjects/Game Event")]
public class GameEvent : ScriptableObject
{
	protected UnityEvent listener;

	public virtual void Add(UnityAction call)
	{
		listener.AddListener(call);
	}

	public virtual void Raise()
	{
		listener.Invoke();
	}

}