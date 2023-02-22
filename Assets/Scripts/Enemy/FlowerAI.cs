using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAI : Damageable
{

    [SerializeField] Transform flowerBody;
    [SerializeField] Transform flowerHead;

    [SerializeField] Transform projectile;
    [SerializeField] Transform attackPoint;

    [SerializeField] Animator animator;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference attackSound;

    [HideInInspector]
    public Transform player;
    

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        player = WorldState.instance.player.transform;

    }

    // Update is called once per frame
    void Update()
    {
        //adjust this
        animator.Play("Flower_Idle.anim");

        if ((player.position - this.transform.position).magnitude < 100f)
        {
            animator.Play("Flower_Attack.anim");
            FlowerRotate();
        }
    }

    void FlowerRotate()
    {
        Quaternion bodyAdjust = new Quaternion(0, transform.rotation.y, 0,0);
        Quaternion headAdjust = new Quaternion(transform.rotation.x, 0,0,0);

        Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position);
        //have body and head turn towards player(y-axis for body, x-axis for head)
        
        flowerBody.localRotation = new Quaternion(0f, lookRotation.y, 0f, 0f);//I dont know why its not rotating
        //flowerBody.localRotation = Quaternion.LookRotation(player.position - transform.position) * bodyAdjust;

        flowerHead.localRotation = new Quaternion(lookRotation.x, 0f, 0f, 0f);
        //flowerHead.eulerAngles = new Vector3(0, flowerHead.eulerAngles.y);

    }

    void FlowerAttack()
    {
        Transform newProjectile = Instantiate(projectile, flowerHead.position, flowerHead.rotation);

        newProjectile.GetChild(0).GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);

    }
}
