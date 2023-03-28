using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
	[SerializeField] protected GameObject door;
	[SerializeField] PuzzleKey[] keys;
	[SerializeField] float doorMinY;
	[SerializeField] float doorDropRate = 15;
	[SerializeField] ParticleSystem dustParticles;
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
		else
        {
			if(door.transform.localPosition.y >= doorMinY)
            {
				door.transform.localPosition = new Vector3(door.transform.localPosition.x, door.transform.localPosition.y - doorDropRate * Time.deltaTime, door.transform.localPosition.z);
            }
        }
	}
	public void OpenDoor()
	{
		//for now, destroy door. can have an animation or something more pretty later
		isOpened = true;
		WorldState.instance.AddActivatedDoor(this);
		dustParticles.Play();
	    
	}
}
