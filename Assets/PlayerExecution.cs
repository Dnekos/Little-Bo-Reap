using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExecution : MonoBehaviour
{
    [Header("Execution Command Variables")]
    [SerializeField] float executeRadius = 10f;
    [SerializeField] LayerMask enemyExecuteLayer;
    public List<EnemyAI> executableEnemies;

    bool canExecute;
    EnemyAI enemyToExecute;
    Transform targetPos;
    public PlayerInput inputs;
    bool isExecuting;

    public void OnExecute(InputAction.CallbackContext context)
    {
        //when you press the execute key
        if(context.started)
        {
            //get all enemies in radius, if we get an executable enemy then we can execute
            Collider[] hits = Physics.OverlapSphere(transform.position, executeRadius, enemyExecuteLayer);
            foreach(Collider hit in hits)
            {
                if (hit.GetComponent<EnemyAI>().isExecutable) executableEnemies.Add(hit.GetComponent<EnemyAI>());
                canExecute = true;
            }

            //execute first enemy in list, play animations from execute scriptable object and start execute coroutine
            if(canExecute)
            {
                enemyToExecute = executableEnemies[0];
                targetPos = enemyToExecute.executePlayerPos;

                GetComponent<PlayerAnimationController>().playerAnimator.Play(enemyToExecute.execution.playerAnimation);
                enemyToExecute.GetComponent<Animator>().Play(enemyToExecute.execution.enemyAnimation);

                Debug.Log(enemyToExecute.execution.executionLength);
                StartCoroutine(ExecuteEnemy(enemyToExecute.execution.executionLength));

                canExecute = false;
            }
        }
     
    }

    //if executing, figure out positions. TODO
    private void Update()
    {
        if(isExecuting)
        {
            transform.LookAt(enemyToExecute.transform.position);
            transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
            enemyToExecute.transform.LookAt(transform.position);
            enemyToExecute.transform.eulerAngles = new Vector3(0, enemyToExecute.transform.rotation.eulerAngles.y, 0);
        }
        
    }

    IEnumerator ExecuteEnemy(float length)
    {
        StartExecution();
        yield return new WaitForSeconds(length);
        EndExecution();
    }

    //make player invulnerable during executions!!
    public void StartExecution()
    {
        isExecuting = true;
        inputs.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("PlayerInvulnerable");
    }

    public void EndExecution()
    {
        isExecuting = false;
        enemyToExecute.Execute();
        
        inputs.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Player");

        executableEnemies.Clear();
    }
}
