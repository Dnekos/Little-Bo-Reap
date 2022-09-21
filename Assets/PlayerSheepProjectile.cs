using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheepProjectile : MonoBehaviour
{
    [Header("Launch Projectile Variables")]
    [SerializeField] float launchForce = 2500f;
    [SerializeField] float launchForceLift = 250f;
    [SerializeField] float lifeTime = 10f;

    Rigidbody rb;

    void Awake()
    {
        Destroy(gameObject, lifeTime);
        rb = GetComponent<Rigidbody>();
    }

    public void LaunchProjectile()
    {
        rb.AddForce(transform.forward * launchForce + transform.up * launchForceLift);
        rb.AddTorque(100f, 100f, 100f);
    }
}
