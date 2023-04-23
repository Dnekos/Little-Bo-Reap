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
	[SerializeField] FMODUnity.EventReference doorSounds;
	bool isPlaying = false ;
	bool isStoped = false;
	FMOD.Studio.Bus myBus;
	float doorTime;
    private void Start()
    {
		myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay/UponDeath/DoorDrop");
	}
    virtual protected void Update()
    {
		Debug.Log(doorTime);
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
			if (door.transform.localPosition.y >= doorMinY)
			{
				door.transform.localPosition = new Vector3(door.transform.localPosition.x, door.transform.localPosition.y - doorDropRate * Time.deltaTime, door.transform.localPosition.z);
				if (!isPlaying)
				{
					FMODUnity.RuntimeManager.PlayOneShot(doorSounds,transform.localPosition);
					isPlaying = true;
					
					
				}
				doorTime = doorTime - 1;
				
			}
			if (doorTime <= 0 && isOpened && !isStoped)
            {
				myBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
				//isPlaying = false;
				isStoped = true;
				

			}
        }

	}
	virtual public void OpenDoor()
	{
		//for now, destroy door. can have an animation or something more pretty later
		isOpened = true;
		WorldState.instance.AddActivatedDoor(this);
		dustParticles.Play();
		doorTime = doorDropRate * 60;
	    
	}
	
}
