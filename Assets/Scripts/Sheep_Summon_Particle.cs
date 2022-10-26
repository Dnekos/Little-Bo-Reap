using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep_Summon_Particle : MonoBehaviour
{
    [SerializeField] GameObject crackAndPoof;
    GameObject sheepToSpawn;
    SheepTypes sheepType;
    float lerpSpeed = 5f;
    Vector3 spawnPoint;
    PlayerSheepAbilities player;
    bool canSpawn = true;

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

            //spawn sheep
            Instantiate(crackAndPoof, spawnPoint, Quaternion.identity);
            var sheep = Instantiate(sheepToSpawn, spawnPoint, Quaternion.identity) as GameObject;
            player.GetSheepFlock(sheepType).Add(sheep.GetComponent<PlayerSheepAI>());
           
            //determine if it's a black sheep
            float rand = Random.Range(0f, 100f);
            if (rand <= player.summonBlackSheepPercent || player.gothMode.isGothMode) sheep.GetComponent<PlayerSheepAI>().isBlackSheep = true;
        }
    }
}
