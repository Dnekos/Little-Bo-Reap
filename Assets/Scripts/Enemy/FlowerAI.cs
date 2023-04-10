using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAI : EnemyBase
{
	[Header("Flower")]
	[SerializeField, Tooltip("how close for the flower to start locking in on player")] float Range = 70f;
    [SerializeField] Transform flowerBody;
    [SerializeField] Transform attackPoint;


    [Header("Attacks"), SerializeField] protected EnemyAttack activeAttack;
	[SerializeField, Range(0, 1), Tooltip("amplitude of randomness for cooldown")] float cooldownRange = 0;

	bool canAttack = true;
	Animator anim;

	[Header("Placement")]
	[SerializeField] LayerMask groundMask;
	[SerializeField] float startpointOffset;

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

		// dont shoot automatically
		StartCoroutine(RunFirstCooldown());

		// make sure they're on the ground
		RaycastHit info;
		if (Physics.Raycast(transform.position + Vector3.up * startpointOffset, Vector3.down, out info, startpointOffset + 20, groundMask, QueryTriggerInteraction.Ignore))
			transform.position = info.point;
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
		yield return new WaitForSeconds(activeAttack.MaxCooldown * (1 + Random.Range(-cooldownRange, cooldownRange)));

		canAttack = true;
	}
	/// <summary>
	/// this one has some randomness to make the flowers not feel too synched
	/// </summary>
	/// <returns></returns>
	IEnumerator RunFirstCooldown()
	{
		canAttack = false;
		yield return new WaitForSeconds(activeAttack.MaxCooldown + Random.Range(0, activeAttack.MaxCooldown));
		canAttack = true;
	}
}
