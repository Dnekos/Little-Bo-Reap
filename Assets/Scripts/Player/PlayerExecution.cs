using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExecution : MonoBehaviour
{
    [Header("Interaction Variables")]
    [SerializeField] float interactRadius = 5f;
    [SerializeField] LayerMask interactLayer;
    Interactable interactableObject;
    bool interactableInRange;

    [Header("Execution Command Variables")]
    [SerializeField] float executeRadius = 10f;
    [SerializeField] LayerMask enemyExecuteLayer;
    public List<EnemyAI> executableEnemies;

    [Header("Pet Sheep Variables")]
    [SerializeField] string petAnimation;
    [SerializeField] float petMaxDistance = 7.5f;
    float currentPetDistance;
    PlayerSheepAI sheepToPet;
    bool isPetting;

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

            //look for an interactable object first
            Collider[] interactables = Physics.OverlapSphere(transform.position, interactRadius, interactLayer);
            foreach(Collider interactable in interactables)
            {
                if(interactable.GetComponent<Interactable>().canInteract)
                {
                    interactableInRange = true;
                    interactableObject = interactable.GetComponent<Interactable>();
                }
            }

            //get all enemies in radius, if we get an executable enemy then we can execute
            //execution ovverrides interactable
            Collider[] hits = Physics.OverlapSphere(transform.position, executeRadius, enemyExecuteLayer);
            foreach(Collider hit in hits)
            {
                if (hit.GetComponent<EnemyAI>().isExecutable)
                {
                    executableEnemies.Add(hit.GetComponent<EnemyAI>());
                    interactableInRange = false;
                    canExecute = true;
                }
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
            else if(interactableInRange)
            {
                interactableObject.Interact();
                interactableInRange = false;
            }
            //else check to pet nearest sheep of current flock! :D
            else
            {
                sheepToPet = null;
                currentPetDistance = 999;

                //this is all for sean don't think too hard about it
                for(int i = 0; i < GetComponent<PlayerSheepAbilities>().GetSheepFlock(GetComponent<PlayerSheepAbilities>().currentFlockType).Count; i++)
                {
                    if (Vector3.Distance(transform.position, GetComponent<PlayerSheepAbilities>().GetSheepFlock(GetComponent<PlayerSheepAbilities>().currentFlockType)[i].transform.position) < petMaxDistance
                        && currentPetDistance > Vector3.Distance(transform.position, GetComponent<PlayerSheepAbilities>().GetSheepFlock(GetComponent<PlayerSheepAbilities>().currentFlockType)[i].transform.position))
                    {
                        currentPetDistance = Vector3.Distance(transform.position, GetComponent<PlayerSheepAbilities>().GetSheepFlock(GetComponent<PlayerSheepAbilities>().currentFlockType)[i].transform.position);
                        sheepToPet = GetComponent<PlayerSheepAbilities>().GetSheepFlock(GetComponent<PlayerSheepAbilities>().currentFlockType)[i];
                    }
                }

                if(sheepToPet != null)
                {
                    transform.LookAt(sheepToPet.transform.position);
                    transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
                    GetComponent<PlayerAnimationController>().playerAnimator.Play(petAnimation);
                    sheepToPet.PetSheep();
                }
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
