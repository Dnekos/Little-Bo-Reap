using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAI : EnemyBase
{
	[Header("Flower")]
	[SerializeField, Tooltip("how close for the flower to start locking in on player")] float Range = 70f;
    [SerializeField] Transform flowerBody;
    [SerializeField] Transform attackPoint;

    [SerializeField] protected EnemyAttack activeAttack;
	bool canAttack = true;
	Animator anim;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference attackSound;

    [HideInInspector]
    public Transform player;
    

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        player = WorldState.instance.player.transform;
		anim = GetComponent<Animator>();
	}

    // Update is called once per frame
    void Update()
    {

		//adjust this(make it its own function)
		if ((player.position - this.transform.position).magnitude < Range)
		{
			FlowerRotate();
			if (canAttack)
			{
				//anim.Play();
				activeAttack.PerformAttack(anim);
				StartCoroutine(RunCooldown());
			}
		}
    }

    void FlowerRotate()
    {
        Quaternion bodyLookRotation = Quaternion.LookRotation(player.position - flowerBody.position, transform.eulerAngles);

        //have body and head turn towards player(y-axis for body, x-axis for head)
        flowerBody.eulerAngles = Quaternion.Euler(bodyLookRotation.eulerAngles.x, bodyLookRotation.eulerAngles.y, 0f).eulerAngles;
    }


    public void SpawnProjectile()
    {
		if (activeAttack != null)
			activeAttack.SpawnObject(attackPoint.position, attackPoint.rotation);
    }

	IEnumerator RunCooldown()
	{
		canAttack = false;
		yield return new WaitForSeconds(activeAttack.MaxCooldown);
		canAttack = true;
	}
}
