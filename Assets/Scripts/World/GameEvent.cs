using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "ScriptableObjects /Game Event")]
public class GameEvent : ScriptableObject
{
	public UnityEvent listener;



	public virtual void Raise()
	{
		listener.Invoke();
	}

}