using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellDistract : MonoBehaviour
{
    [SerializeField] float range;
    public bool distracting = false;
    List<EnemyAI> distractedEnemies = new List<EnemyAI>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float currentSheepCount = GetComponent<SheepConstruct>().GetSheepCount();
        if (currentSheepCount <= 0)
        {
            EndDistract();
        }
        distracting = currentSheepCount > 0; 
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Enemy" && other.GetComponent<EnemyAI>() != null && other.GetComponent<EnemyAI>().distracted == false && distracting)
        {            
            other.GetComponent<EnemyAI>().distracted = true;
            other.GetComponent<EnemyAI>().bellLoc = this.transform.position;
            distractedEnemies.Add(other.GetComponent<EnemyAI>());
        }
    }

    public void EndDistract()
    {
        foreach(EnemyAI enemy in distractedEnemies)
        {
            enemy.distracted = false;
        }
    }

    private void OnDestroy()
    {
        EndDistract();
    }
}
