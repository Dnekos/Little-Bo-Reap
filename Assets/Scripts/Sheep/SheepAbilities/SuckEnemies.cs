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
    [SerializeField] float inVortexRotSpeed = 0.01f;
    [SerializeField] float inVortexLerpSpeed = 0.6f;
    [SerializeField] bool inVortexLerpUseDt = false;

	[Header("Finisher Damage")]
	[SerializeField] Attack ChuckAtack;

    [Header("Chuck Timings")]
    [SerializeField] float spinTime;
    [SerializeField] float suckResistDuration;

    [Header("VFX")]
    [SerializeField] ParticleSystem particleEnd;

    List<KeyValuePair<GameObject, float>> enemiesStuck = new List<KeyValuePair<GameObject,float>>();
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
    }

	private void LateUpdate()
	{
		Spin();

		Suck();

	}

	private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, suckDistance);
    }

    private void OnDisable()
    {
        particleEnd.Play(true);
    }
    public void Suck()
    {
        foreach (KeyValuePair<GameObject, float> enemy in enemiesStuck)
        {
            if (enemy.Key == null)
            {
                continue;
            }
            float dist = Vector3.Distance(enemy.Key.transform.position, this.transform.position);
            if (dist <= suckDistance && dist >= spinDistance)
            {
                Vector3 pushForce = (transform.position - enemy.Key.transform.position).normalized * suckForce;
				//enemy.GetComponent<Rigidbody>().AddForce(pushForce);
				enemy.Key.transform.position = Vector3.Lerp(enemy.Key.transform.position, new Vector3(enemy.Key.transform.position.x + pushForce.x,
													   enemy.Key.transform.position.y,
													   enemy.Key.transform.position.z + pushForce.z), 0.6f);
                //stun the enemies
                Attack stunAttack = new Attack();
                stunAttack.damage = 0;
                stunAttack.DealsHitstun = true;
                stunAttack.ShowNumber = false;
                enemy.Key.GetComponent<EnemyAI>().TakeDamage(stunAttack, Vector3.zero);
            }
        }
    }

    public void Spin()
    {
        Transform player = WorldState.instance.player.transform;

        for (int i = 0; i < enemiesStuck.Count; i++)
        {
            if (enemiesStuck[i].Key == null)
            {
                enemiesStuck.RemoveAt(i);
                i--;
                continue;
            }
            if (Vector3.Distance(player.transform.position, enemiesStuck[i].Key.transform.position) < spinDistance + 2)//defendRotateDistance - 2f)
            {
                //arbitrary number to increase size so that the enemy doesnt teleport around anymore.
                float radAngle = (i / ((float)enemiesStuck.Count + 20)) * Mathf.PI * 2;

                Vector3 dest = player.transform.position
                    + new Vector3(Mathf.Sin(radAngle + Time.time * inVortexRotSpeed), 0, Mathf.Cos(radAngle + Time.time * inVortexRotSpeed)) * spinDistance;

                enemiesStuck[i].Key.transform.position = Vector3.Lerp(enemiesStuck[i].Key.transform.position, dest, inVortexLerpSpeed * (inVortexLerpUseDt ? Time.deltaTime : 1));
            }
            else
            {
                enemiesStuck[i].Key.transform.position = Vector3.Lerp(enemiesStuck[i].Key.transform.position, player.transform.position, inVortexRotSpeed * Time.deltaTime);
            }
            enemiesStuck[i] = new KeyValuePair<GameObject, float> (enemiesStuck[i].Key, enemiesStuck[i].Value + Time.deltaTime);
            if(enemiesStuck[i].Value > spinTime)
            {
                ChuckEnemy(i);
                i--;
            }
        }
    }

    public void BlackHoleChuckALL()
    {
        float breaker = 0;
        while(enemiesStuck.Count > 0)
        {
            breaker++;
            ChuckEnemy(0);
            if(breaker > 1000)
            {
                Debug.Log("infinite loop in black hole");
                break;
            }
        }
        enemiesStuck.Clear();
        this.gameObject.SetActive(false);
    }

    private void ChuckEnemy(int index)
    {
        if (enemiesStuck[index].Key == null && !enemiesStuck[index].Key.activeInHierarchy)
        {
            enemiesStuck.RemoveAt(index);
            return;
        }
        if (Vector3.Distance(enemiesStuck[index].Key.transform.position, this.transform.position) <= suckDistance)
        {
            Vector3 knockDir = (enemiesStuck[index].Key.transform.position - transform.position);

			EnemyBase enemy = enemiesStuck[index].Key.GetComponent<EnemyAI>();

			enemy.TakeDamage(ChuckAtack, knockDir);
			enemy.SuckResistTimer(suckResistDuration);
            enemiesStuck.RemoveAt(index);
            //body>().AddForce(knockForce);            
        }
        else
        {
            enemiesStuck.RemoveAt(index);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            if (keyValueListContains(other.gameObject).Key == null)
            {
                if (!other.gameObject.GetComponent<EnemyBase>().suckResistant)
                {
                    enemiesStuck.Add(new KeyValuePair<GameObject, float>(other.gameObject, 0));
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            if (keyValueListContains(other.gameObject).Key == null)
            {
                if (!other.gameObject.GetComponent<EnemyBase>().suckResistant)
                {
                    enemiesStuck.Add(new KeyValuePair<GameObject, float>(other.gameObject, 0));
                }
            }
            //if we contain it and its suck resis then its prob execute remove it from the list
            else if (other.gameObject.GetComponent<EnemyBase>().suckResistant)
            {
                enemiesStuck.Remove(new KeyValuePair<GameObject, float>(other.gameObject, 0));
            }
            
        }
    }

    private KeyValuePair<GameObject, float> keyValueListContains(GameObject objToFind)
    {
        foreach (KeyValuePair<GameObject, float> enemy in enemiesStuck)
        {
            if(enemy.Key == objToFind)
            {
                return enemy;
            }
        }
        return new KeyValuePair<GameObject, float>(null,0);
    }
}
