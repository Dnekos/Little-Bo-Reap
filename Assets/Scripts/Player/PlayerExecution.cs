using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExecution : MonoBehaviour
{
    [Header("Interaction Variables")]
    [SerializeField] float interactRadius = 5f;
    [SerializeField] LayerMask interactLayer;

    [Header("Execution Command Variables")]
    [SerializeField] float executeRadius = 10f;
	[SerializeField] Vector3 CameraOffset;
    [SerializeField] LayerMask enemyExecuteLayer;

    [Header("Pet Sheep Variables")]
    [SerializeField] string petAnimation;
    [SerializeField] float petMaxDistance = 7.5f;
    float currentPetDistance;

	[Header("General Need-to-knows")]
	[SerializeField] GameEvent EndAim;
    EnemyAI enemyToExecute;
   // Transform targetPos;
    public PlayerInput inputs;
    bool isExecuting;

	// components
	PlayerAnimationController anim;
	PlayerSheepAbilities flocks;
    PlayerMovement movement;
	Rigidbody rb;

	public void OnPet(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			// check to pet nearest sheep of current flock! :D
			PlayerSheepAI sheepToPet = null;
			currentPetDistance = Mathf.Infinity;

			//this is all for sean don't think too hard about it
			for (int j = 0; j < 3; j++)
			{
				for (int i = 0; i < flocks.GetActiveSheep(j).Count; i++)
				{
					float sqrDist = Vector3.SqrMagnitude(transform.position - flocks.GetActiveSheep(j)[i].transform.position);
					if (sqrDist < petMaxDistance && currentPetDistance > sqrDist)
					{
						currentPetDistance = sqrDist;
						sheepToPet = flocks.GetActiveSheep(j)[i];
					}
				}
			}


			if (sheepToPet != null)
			{
				transform.LookAt(sheepToPet.transform.position);
				transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);

				anim.playerAnimator.Play(petAnimation);
				sheepToPet.PetSheep();
			}
		}
	}

	public void OnExecute(InputAction.CallbackContext context)
    {
        //when you press the execute key and are not in an execution 
        if(context.started && !isExecuting)
        {
            //look for an interactable object first
            Collider[] interactables = Physics.OverlapSphere(transform.position, interactRadius, interactLayer);
			for (int i = 0; i < interactables.Length; i++)
			{
				Interactable interactComp = interactables[i].GetComponent<Interactable>();

				if (interactComp != null && interactComp.canInteract)
				{
					interactComp.Interact();
					// only trigger one interactable or enemey
					return;
				}
			}

            //get all enemies in radius, if we get an executable enemy then we can execute
            //execution ovverrides interactable
            Collider[] hits = Physics.OverlapSphere(transform.position, executeRadius, enemyExecuteLayer);
			for (int i = 0; i < hits.Length; i++)
            {
				EnemyAI enemy = hits[i].GetComponent<EnemyAI>();
				if (enemy != null && enemy.isExecutable)
                {
					enemyToExecute = enemy;
					//targetPos = enemyToExecute.executePlayerPos;
					transform.position = enemyToExecute.executePlayerPos.transform.position;

					// a dirty way to get it to stop what its doing
					anim.playerAnimator.Rebind();

					// play execution animations
					anim.playerAnimator.Play(enemyToExecute.execution.playerAnimation, 0, 0);
					enemyToExecute.GetComponent<Animator>().Play(enemyToExecute.execution.enemyAnimation);
					enemyToExecute.getExecuteTrigger().SetActive(false);//disables ability to execute twice on accident

					Debug.Log(enemyToExecute.execution.executionLength);
					StartCoroutine(ExecuteEnemy(enemyToExecute.execution.executionLength));
					EndAim.Raise();

					return;
				}
            }

			// if neither enemy nor interactable are found, recall sheep
			flocks.OnRecallSheep(context);
        }
     
    }

	private void Start()
	{
		anim = GetComponent<PlayerAnimationController>();
		flocks = GetComponent<PlayerSheepAbilities>();
        movement = GetComponent<PlayerMovement>();
		rb = GetComponent<Rigidbody>();
	}

	//if executing, figure out positions. TODO
	private void Update()
    {
        if(isExecuting)
        {
			rb.velocity = Vector3.zero;
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
        inputs.currentActionMap.Disable();
        movement.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("PlayerInvulnerable");
    }

    public void EndExecution()
    {
        isExecuting = false;
        enemyToExecute.Execute();

		inputs.currentActionMap.Enable();
		movement.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
}
