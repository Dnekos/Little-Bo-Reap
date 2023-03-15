using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
	[SerializeField] protected GameObject door;
	[SerializeField] PuzzleKey[] keys;
	[HideInInspector] public bool isOpened = false;
	virtual protected void Update()
	{
		if (!isOpened)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (!keys[i].isOpened)
					return;
			}

			// if we haven't returned, then all pinwheels are spinning
			OpenDoor();
		}
	}
	public void OpenDoor()
	{
		//for now, destroy door. can have an animation or something more pretty later
		isOpened = true;
		WorldState.instance.AddActivatedDoor(this);
		Destroy(door);
	}
}
