using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyAI : EnemyBase
{
    Dictionary<int, float> Cooldowns;

    [SerializeField] public int flightPathIndex = 0;
    [SerializeField] Transform frogPrefab;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRadius;
    [SerializeField] float attackDamage;

    [SerializeField] protected EnemyAttack activeAttack;
    [SerializeField] Animator anim;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference attackSound;
    [SerializeField] FMODUnity.EventReference Flying;

    [HideInInspector]public Transform player;
    [HideInInspector]public bool attacking;//checks animator to see if we are attacking

    [Header("Health")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float fallForce = 30;
    [SerializeField] float pauseBeforeSpiral = 1;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        Cooldowns = new Dictionary<int, float>();
        player = WorldState.instance.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForAttack();
    }

    public void RunAttack(EnemyAttack atk)
    {
        atk.PerformAttack(anim);
        activeAttack = atk;
    }


    public void CheckForAttack()
    {
        //checks to see if player and sheep are underneath the enemy

        //fires a raycast down, then at the point of collision with the ground
        //(or any other terrain) we create an overlap sphere which checks 
        //if the player is in that sphere, and if true, then execute attack
        if(attacking == true || Health <= 0)
        {
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            if((groundMask & 1 << hit.collider.gameObject.layer) == 1)//ground layer
            {
                Vector3 point = hit.point;
                Collider[] hitColliders = Physics.OverlapSphere(point, attackRadius);

                foreach(Collider collider in hitColliders)
                {
                    if(collider.gameObject.tag == "Player" || collider.gameObject.tag == "Sheep")
                    {
                        RunAttack(activeAttack);
                        return;
                    }
                }
            }
        }

    }

    public void SpawnFrog()
    {
        if (activeAttack != null)
        {
            activeAttack.SpawnObject(attackPoint.position, attackPoint.rotation);
            activeAttack.damage = attackDamage;
        }
    }

    //protected IEnumerator SpawnEnemy(GameObject enemy, GameObject particle, Vector3 pos)
    //{

    //    Instantiate(particle, pos, transform.rotation, transform);
    //    yield return new WaitForSeconds(enemy.GetComponent<EnemyAI>().SpawnWaitTime);
    //    GameObject newFrog = Instantiate(enemy, pos, transform.rotation, transform);
    //    newFrog.GetComponent<EnemyAI>().ToChase();
    //    //newFrog.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);//temporary
    //}


    #region Health
    protected override void OnDeath()
    {
        anim.SetTrigger("Death");
        GetComponent<SplineFollower>().enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        StartCoroutine(DeathSpiral());
    }

    IEnumerator DeathSpiral()
    {
        yield return new WaitForSeconds(pauseBeforeSpiral);

        while (gameObject != null || !gameObject.activeInHierarchy)
        {
            yield return new WaitForFixedUpdate();
            rb.AddForce(Vector3.down * fallForce, ForceMode.Acceleration);

        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (6 == collision.gameObject.layer && Health <= 0)
        {
            base.OnDeath();
        }
    }
    #endregion
}
