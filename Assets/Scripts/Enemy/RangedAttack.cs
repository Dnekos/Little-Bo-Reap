using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [SerializeField] Attack BossRangedAttack;

    List<Damageable> hitTargets;

    Vector3 origPos;

    [SerializeField]float maxTimeAlive = 2.5f;
	[SerializeField] FMODUnity.EventReference explosion;


	float currentTimeAlive = 0;

    //small cheat
    private Transform player;

    [SerializeField] ParticleSystem destroyExplosion;
    [SerializeField] SphereCollider col;
    [SerializeField] ParticleSystem[] attackParticles;



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
        if(6 == other.gameObject.layer)
        {
            //Vector3 tempPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            //Quaternion tempRot = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
            //Instantiate(destroyExplosion, tempPos, tempRot);
            //Destroy(gameObject);
            //WorldState.instance.pools.DequeuePooledObject(destroyExplosion.gameObject, tempPos, tempRot);
            StartCoroutine(DestroyProjectile());
        }

    }

    IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(0.1f);
        Vector3 tempPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Quaternion tempRot = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);

		FMODUnity.RuntimeManager.PlayOneShot(explosion, transform.position);

		Instantiate(destroyExplosion, tempPos, tempRot);
        Destroy(gameObject);

        //collider.enabled = false;
        //foreach (ParticleSystem ps in attackParticles)
        //{
        //    ps.Stop();
        //}

        //gameObject.SetActive(false);
    }

    private void FireProjectile()
    {
        Vector3 fireDirection = new Vector3(player.position.x, player.position.y + 10f, player.position.z) - transform.position;
        this.GetComponent<Rigidbody>().AddForce(fireDirection.normalized * (70f * fireDirection.magnitude));
        
    }
}
