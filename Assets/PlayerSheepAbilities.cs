using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerSheepAbilities : MonoBehaviour
{
    [Header("Base Sheep Variables")]
    [SerializeField] List<PlayerSheepAI> activeSheep;
    [SerializeField] GameObject sheepPrefab;

    [Header("Sheep Summon Variables")]
    [SerializeField] float amountToSummon = 3f;
    [SerializeField] float summonRadius = 20f;
    [SerializeField] float summonIntervalMin = 0f;
    [SerializeField] float summonIntervalMax = 0.5f;
    [SerializeField] float summonCooldown = 5f;
    bool canSummonSheep = true;
   
    public void OnSummonSheep(InputAction.CallbackContext context)
    {
        if(context.performed && canSummonSheep)
        {
            //disallow summoning
            canSummonSheep = false;

            //delete all active sheep
            for(int i = 0; i < activeSheep.Count; i++)
            {
                activeSheep[i].KillSheep();
            }
            activeSheep.Clear();

            //summon sheep!
            for(int i = 0; i < amountToSummon; i++)
            {
                StartCoroutine(SummonSheep());
            }

            //start cooldown
            StartCoroutine(SummonSheepCooldown());
        }
    }
    IEnumerator SummonSheep()
    {
        //get random interval
        float summonDelay = Random.Range(summonIntervalMin, summonIntervalMax);

        yield return new WaitForSeconds(summonDelay);

        //get random point inside summoning radius
        Vector3 summonPosition = Vector3.zero;
        Vector3 randomPosition = Random.insideUnitSphere * summonRadius;
        randomPosition += transform.position;

        //if inside navmesh, spawn sheep!
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, summonRadius, 1))
        {
            //get spawn position
            summonPosition = hit.position;

            //spawn sheep
            var sheep = Instantiate(sheepPrefab, summonPosition, Quaternion.identity) as GameObject;
            activeSheep.Add(sheep.GetComponent<PlayerSheepAI>());
        }
        else
        {
            Debug.LogWarning("Sheep could not be summoned! Are you too far from a navmesh?");
        }
    }
    IEnumerator SummonSheepCooldown()
    {
        yield return new WaitForSeconds(summonCooldown);
        canSummonSheep = true;
    }
    
}
