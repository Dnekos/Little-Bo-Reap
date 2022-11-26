using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] Attack killAttack;

    private void OnTriggerEnter(Collider other)
    {
		other.GetComponent<Damageable>()?.ForceKill();
    }
}
