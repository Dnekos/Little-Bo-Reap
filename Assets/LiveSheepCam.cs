using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveSheepCam : MonoBehaviour
{
	[SerializeField] Vector3 offset;
	PlayerSheepAbilities player;

    // Start is called before the first frame update
    void Start()
    {
		player = WorldState.instance.player.GetComponent<PlayerSheepAbilities>();
	}

    // Update is called once per frame
    void Update()
    {
		if (player.leaderSheep.Count > 0)
		{
			transform.position = player.leaderSheep[0].transform.position + Vector3.Project(offset, player.leaderSheep[0].transform.forward).normalized * offset.magnitude;
			transform.LookAt(player.leaderSheep[0].transform);
		}
	}
}
