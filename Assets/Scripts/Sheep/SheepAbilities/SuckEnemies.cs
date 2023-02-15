using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuckEnemies : MonoBehaviour
{
    [SerializeField] float suckDistance;
    [SerializeField] float suckForce;
    [SerializeField] float spinDistance;
    [SerializeField] float finishDamage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemies)
        {
            if(Vector3.Distance(enemy.transform.position,this.transform.position) <= suckDistance)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, suckDistance);
    }

    public void stopSucking()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (Vector3.Distance(enemy.transform.position, this.transform.position) <= suckDistance)
            {
                Attack finishAttack = new Attack();
                finishAttack.damage = finishDamage;
                finishAttack.DealsHitstun = true;

                Vector3 knockForce = (enemy.transform.position - transform.position).normalized * 1000000;

                enemy.GetComponent<EnemyAI>().TakeDamage(finishAttack, knockForce);
            }
        }
        this.gameObject.SetActive(false);
    }
}
