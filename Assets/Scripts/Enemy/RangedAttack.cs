using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [SerializeField] Attack BossRangedAttack;

    List<Damageable> hitTargets;

    Vector3 origPos;

    [SerializeField]float maxTimeAlive = 2.5f;
    [SerializeField] FMODUnity.EventReference whistle;
    float currentTimeAlive = 0;

    //small cheat
    public Transform player;
   

    // Start is called before the first frame update
    void Start()
    {
        hitTargets = new List<Damageable>();

        origPos = transform.position;
        player = WorldState.instance.player.transform;

        FireProjectile();
    }

    // Update is called once per frame
    void Update()
    {
        currentTimeAlive += Time.deltaTime;

        if (currentTimeAlive >= maxTimeAlive)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if not damageable
        //    return;

        Vector3 flattenedOtherPos = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
        Damageable targetHealth = other.GetComponent<Damageable>();
        if (targetHealth != null && !other.isTrigger)
        {
            //hit target takes damage
            hitTargets.Add(targetHealth);
            Debug.Log(other.gameObject.name + "hit by Ranged Attack");
            targetHealth.TakeDamage(BossRangedAttack, (flattenedOtherPos - origPos).normalized);
        }
    }

    private void FireProjectile()
    {
        Vector3 fireDirection = new Vector3(player.position.x, player.position.y + 10f, player.position.z) - transform.position;
        this.GetComponent<Rigidbody>().AddForce(fireDirection.normalized * (70f * fireDirection.magnitude));
        FMODUnity.RuntimeManager.PlayOneShot(whistle, player.transform.position);
        
    }
}
