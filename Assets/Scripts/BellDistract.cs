using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellDistract : MonoBehaviour
{
    [SerializeField] float range;
    public bool isDistracting = false;
    List<EnemyAI> distractedEnemies = new List<EnemyAI>();
	[SerializeField] GameObject bellparticles;

	SheepConstruct construct;
	// Start is called before the first frame update
	void Start()
    {
		construct = GetComponent<SheepConstruct>();

	}//REVIEW: if we're not using it, we can get rid of it to clean up the script

	// Update is called once per frame
	void Update()
	{
		float currentSheepCount = construct.GetSheepCount();

		isDistracting = currentSheepCount > 0;
		bellparticles.SetActive(isDistracting);
		if (!isDistracting)
		{
			EndDistract();
		}
	}

    private void OnTriggerStay(Collider other)
    {
		EnemyAI ai = other.GetComponent<EnemyAI>();
		if (other.tag == "Enemy" && ai != null && other.GetComponent<EnemyAI>().distracted == false && isDistracting)
        {
            ai.distracted = true;
			ai.bellLoc = this.transform.position;
            distractedEnemies.Add(ai);
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
