using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep_Summon_Particle : MonoBehaviour
{
    [SerializeField] GameObject crackAndPoof;
	[SerializeField] FMODUnity.EventReference dirtSound;
	[SerializeField] int maxSheepBeforeIgnoreParticle = 100;

	GameObject sheepToSpawn;
    [HideInInspector] public SheepTypes sheepType;
    float lerpSpeed = 5f;
    Vector3 spawnPoint;
    PlayerSheepAbilities player;
    bool canSpawn = true;
	public PlayerSheepAI.callSheep removeFunction;

	public void InitSheepParticle(GameObject theSheepToSpawn, float theLerpSpeed, Vector3 theSpawnPoint, PlayerSheepAbilities thePlayer, SheepTypes theSheepType)
    {
        sheepToSpawn = theSheepToSpawn;
        lerpSpeed = theLerpSpeed;
        spawnPoint = theSpawnPoint;
        player = thePlayer;
        sheepType = theSheepType;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, spawnPoint, lerpSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, spawnPoint) <= 0.5 && canSpawn)
        {
            canSpawn = false;

			FMODUnity.RuntimeManager.PlayOneShotAttached(dirtSound, gameObject);

			// only spawn particles if we have low numbers of sheep
			if (player.sheepFlocks[(int)sheepType].activeSheep.Count < maxSheepBeforeIgnoreParticle)
				Instantiate(crackAndPoof, spawnPoint, Quaternion.identity);

			//spawn sheep
			var sheep = Instantiate(sheepToSpawn, spawnPoint, Quaternion.identity).GetComponent<PlayerSheepAI>() as PlayerSheepAI;
            player.GetSheepFlock(sheepType).Add(sheep);
            player.UpdateFlockUI();

			sheep.RemoveSheep = new PlayerSheepAI.callSheep(removeFunction);

            //determine if it's a black sheep
            float rand = Random.Range(0f, 100f);
            if (rand <= player.summonBlackSheepPercent || (sheepType == SheepTypes.BUILD && rand <= player.summonBlackSheepPercent + WorldState.instance.passiveValues.builderCorruptChance) || player.gothMode.isGothMode) 
				sheep.isBlackSheep = true;
        }
    }
}
