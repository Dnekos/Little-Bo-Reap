using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckEnemies : MonoBehaviour
{
    [Header("Suck Variables")]
    [SerializeField] float suckDistance;
    [SerializeField] float suckForce;

    [Header("Spin Randomness")]
    [SerializeField] float spinDistance;
    [SerializeField] float vortexRandCircle = 5f;
    [SerializeField] float vortexMinHeight = 0f;
    [SerializeField] float vortexMaxHeight = 2f;

    [Header("Spin Speed")]
    [SerializeField] float inVortexRotSpeed = 5f;
    [SerializeField] float inVortexLerpSpeed = 0.6f;
    [SerializeField] bool inVortexLerpUseDt = false;

    [Header("Finisher Damage")]
    [SerializeField] float finishDamage = 20;
    [SerializeField] float upKnock;
    [SerializeField] float forwardKnock;

    List<GameObject> enemiesStuck = new List<GameObject>();
                //REVIEW: just a suggestion, but I recently learned about Hash Sets, which have more optimal lookup time than lists,
                //      maybe they could work here since we are using a lot of Count() and Contains() commands, but I haven't used them before so I'm not 100% sure on it

    // Start is called before the first frame update
    void Start()
    {
        //REVIEW: we might want an If statement here to check if the Sphere collider exists
        GetComponent<SphereCollider>().radius = suckDistance;//REVIEW: the sphere collider is something we could pass into this script from engine
    }

    // Update is called once per frame
    void Update()
    {
        Spin();
        Suck();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, suckDistance);
    }

    public void Suck()
    {
        foreach (GameObject enemy in enemiesStuck)
        {
            if (enemy == null)
            {
                continue;
            }
            float dist = Vector3.Distance(enemy.transform.position, this.transform.position);
            if (dist <= suckDistance && dist >= spinDistance)
            {
                Vector3 pushForce = (transform.position - enemy.transform.position).normalized * suckForce;
                //enemy.GetComponent<Rigidbody>().AddForce(pushForce);
                enemy.transform.position = new Vector3(enemy.transform.position.x + pushForce.x,
                                                       enemy.transform.position.y,
                                                       enemy.transform.position.z + pushForce.z);
                //stun the enemies
                Attack stunAttack = new Attack();
                stunAttack.damage = 0;
                stunAttack.DealsHitstun = true;
                stunAttack.ShowNumber = false;
                enemy.GetComponent<EnemyAI>().TakeDamage(stunAttack, Vector3.zero);
            }
        }
    }

    public void Spin()
    {
        Transform player = WorldState.instance.player.transform;
        Debug.Log("following player");

        for (int i = 0; i < enemiesStuck.Count; i++)
        {
            if (enemiesStuck[i] == null)
            {
                continue;
            }
            if (Vector3.Distance(player.transform.position, enemiesStuck[i].transform.position) < spinDistance + 2)//defendRotateDistance - 2f)
            {
                float radAngle = (i / (float)enemiesStuck.Count) * Mathf.PI * 2;
                Vector2 RandomCircle = Random.insideUnitCircle.normalized * vortexRandCircle;

                Vector3 dest = player.transform.position
                    + new Vector3(RandomCircle.x, Random.Range(vortexMinHeight, vortexMaxHeight), RandomCircle.y)
                    + new Vector3(Mathf.Sin(radAngle + Time.time * inVortexRotSpeed), 0, Mathf.Cos(radAngle + Time.time * inVortexRotSpeed)) * spinDistance;

                enemiesStuck[i].transform.position = Vector3.Lerp(enemiesStuck[i].transform.position, dest, inVortexLerpSpeed * (inVortexLerpUseDt ? Time.deltaTime : 1));
            }
            else
            {
                enemiesStuck[i].transform.position = Vector3.Lerp(enemiesStuck[i].transform.position, player.transform.position, inVortexRotSpeed * Time.deltaTime);
            }
        }
    }

    public void stopSucking()
    {
        foreach (GameObject enemy in enemiesStuck)
        {
            if (enemy == null)
            {
                continue;
            }
            if (Vector3.Distance(enemy.transform.position, this.transform.position) <= suckDistance)
            {
                Attack finishAttack = new Attack();
                finishAttack.damage = finishDamage;
                finishAttack.DealsHitstun = true;
                finishAttack.forwardKnockback = forwardKnock;
                finishAttack.upwardKnockback = upKnock;
                Vector3 knockDir = (enemy.transform.position - transform.position);
                //body>().AddForce(knockForce);
                enemy.GetComponent<EnemyAI>().TakeDamage(finishAttack, knockDir);
            }
        }
        enemiesStuck.Clear();
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            if (!enemiesStuck.Contains(other.gameObject))
            {
                enemiesStuck.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            if (!enemiesStuck.Contains(other.gameObject))
            {
                enemiesStuck.Add(other.gameObject);
            }
        }
    }
}
