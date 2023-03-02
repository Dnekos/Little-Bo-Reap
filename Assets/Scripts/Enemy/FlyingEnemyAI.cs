using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyAI : EnemyBase
{
    Dictionary<int, float> Cooldowns;

    [SerializeField] Transform frogPrefab;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRadius;
    [SerializeField] float attackDamage;

    [SerializeField] protected EnemyAttack activeAttack;
    [SerializeField] Animator anim;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference attackSound;

    [HideInInspector]public Transform player;
    [HideInInspector]public bool attacking;//checks animator to see if we are attacking

 


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
        if(attacking == true)
        {
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            if(hit.collider.gameObject.layer == 6)//ground layer
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
            else
            {
                return;
            }
        }

    }

    public void SpawnFrog()
    {
        if (activeAttack != null)
        {
            activeAttack.SpawnObject(attackPoint);
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

}
