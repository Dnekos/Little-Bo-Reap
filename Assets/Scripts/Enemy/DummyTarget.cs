using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyTarget : EnemyBase
{
    [SerializeField] string hitAnimation;

    Vector3 targetPos;
	// Start is called before the first frame update
	override protected void Start()
    {
		base.Start();
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = targetPos;
    }

    public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
    {
        GetComponent<Animator>()?.Play(hitAnimation);
        base.TakeDamage(atk, attackForward, damageAmp, knockbackMultiplier);
    }
}
