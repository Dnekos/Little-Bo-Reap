using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	[SerializeField]
	Animation anim;

	public Transform RespawnPoint;

	[SerializeField] FMODUnity.EventReference Sound;
	[SerializeField]public int checkMusic;

	bool hasEntered = false; // prevents the same checkpoint from being triggered twice

	[Header("CheckpointSkip")]
	public bool addsSheep;
	public GameObject debugSheepAdder;
	WorldState ws;
    private void Start()
    {
		ws = FindObjectOfType<WorldState>();
    }

    private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player" && !hasEntered)
		{
			Debug.Log(other.gameObject + " hit checkpoint at " + transform.position);
			WorldState.instance.SetSpawnPoint(this);

			FMODUnity.RuntimeManager.PlayOneShotAttached(Sound, gameObject);
			CheckpointMusic();

			hasEntered = true;
			anim.Play();
		}
	}
	public void CheckpointMusic()
	{
		if (checkMusic != ws.currentWorldTheme)
		{
			ws.ChangeMusic(checkMusic);
			ws.biomeTheme = checkMusic;
		}
	}
}
