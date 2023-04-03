using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "ScriptableObjects/TMP_SA Event")]
public class ControlSpriteEvent : ScriptableObject
{
	public UnityEvent<TMP_SpriteAsset> listener;

	public virtual void Add(UnityAction<TMP_SpriteAsset> call)
	{
		listener.AddListener(call);
	}

	public virtual void Raise(TMP_SpriteAsset arg)
	{
		listener.Invoke(arg);
	}

}