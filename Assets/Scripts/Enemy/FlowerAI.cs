using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAI : EnemyBase
{
    Dictionary<int, float> Cooldowns;

    [SerializeField] Transform flowerBody;
    [SerializeField] Transform flowerHead;

    [SerializeField] Transform projectile;
    [SerializeField] Transform attackPoint;

    [SerializeField] protected EnemyAttack activeAttack;
    [SerializeField] Animator anim;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference attackSound;

    [HideInInspector]
    public Transform player;
    

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        Cooldowns = new Dictionary<int, float>();
        player = WorldState.instance.player.transform;

    }

    // Update is called once per frame
    void Update()
    {

        //adjust this(make it its own function)
        if ((player.position - this.transform.position).magnitude < 100f)
        {
            FlowerRotate();
            RunAttack(activeAttack);
        }
    }

    void FlowerRotate()
    {
        Quaternion bodyLookRotation = Quaternion.LookRotation(player.position - flowerBody.position, transform.eulerAngles);
        Quaternion headLookRotation = Quaternion.LookRotation(player.position - flowerHead.position, transform.eulerAngles);

        //have body and head turn towards player(y-axis for body, x-axis for head)
        flowerBody.eulerAngles = Quaternion.Euler(0f, bodyLookRotation.eulerAngles.y, 0f).eulerAngles;

        flowerHead.eulerAngles = flowerBody.eulerAngles + Quaternion.Euler(headLookRotation.eulerAngles.x, 0f, 0f).eulerAngles;

    }

    public void RunAttack(EnemyAttack atk)
    {
        atk.PerformAttack(anim);
        activeAttack = atk;
    }

    public void SpawnProjectile()
    {
		if (activeAttack != null)
			activeAttack.SpawnObject(attackPoint.position, attackPoint.rotation);
    }

}
