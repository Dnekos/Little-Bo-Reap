using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyProjectile : MonoBehaviour
{
    [SerializeField] Attack FlyingEnemyProjectileAttack;

    List<Damageable> hitTargets;

    Vector3 origPos;

    [SerializeField] float maxTimeAlive = 2.5f;
    float currentTimeAlive = 0;

    // Start is called before the first frame update
    void Start()
    {
        hitTargets = new List<Damageable>();

        origPos = transform.position;

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

        //hit target takes damage
        hitTargets.Add(targetHealth);
        Debug.Log(other.gameObject.name + "hit by Flying Enemy Projectile Attack");
        targetHealth.TakeDamage(FlyingEnemyProjectileAttack, (flattenedOtherPos - origPos).normalized);
    }


}
