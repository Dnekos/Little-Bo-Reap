using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExecution : MonoBehaviour
{
    [SerializeField] float executeRadius = 10f;
    [SerializeField] LayerMask enemyExecuteLayer;
    bool canExecute;
    //public bool isInExecuteRange;
    public List<EnemyAI> executableEnemies;
    EnemyAI enemyToExecute;
    Transform targetPos;
    public PlayerInput inputs;
    bool isExecuting;

    public void OnExecute(InputAction.CallbackContext context)
    {

        if(context.started)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, executeRadius, enemyExecuteLayer);
            foreach(Collider hit in hits)
            {
                if (hit.GetComponent<EnemyAI>().isExecutable) executableEnemies.Add(hit.GetComponent<EnemyAI>());
                canExecute = true;
            }

            if(canExecute)
            {
                enemyToExecute = executableEnemies[0];
                targetPos = enemyToExecute.executePlayerPos;

                GetComponent<Animator>().Play(enemyToExecute.execution.playerAnimation);
                enemyToExecute.GetComponent<Animator>().Play(enemyToExecute.execution.enemyAnimation);
                canExecute = false;
            }
        }
       // //loop through executeable enemey list and make sure none are null
       // for (int i = 0; i < executableEnemies.Count; i++)
       // {
       //     if (executableEnemies[i] == null) executableEnemies.Remove(executableEnemies[i]);
       // }
       //
       // if (context.started && isInExecuteRange)
       // {
       //     //for now, just kill the one at 0
       //     enemyToExecute = executableEnemies[0];
       //     targetPos = enemyToExecute.executePlayerPos;
       //     isInExecuteRange = false;
       //
       //     GetComponent<Animator>().Play(enemyToExecute.execution.playerAnimation);
       //     enemyToExecute.GetComponent<Animator>().Play(enemyToExecute.execution.enemyAnimation);
       // }
    }

    private void Update()
    {
        if(isExecuting)
        {
            transform.LookAt(enemyToExecute.transform.position);
            transform.position = Vector3.Lerp(transform.position, targetPos.position, 10f * Time.deltaTime);
        }
        
    }

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
