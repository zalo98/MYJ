using UnityEngine;

public class BlueEscapeState : State
{
    private EnemyController controller;
    private MouseBoid mouseBoid;
    private MouseMovement mouseMovement;
    private float escapeSpeed;
    private bool escapePathInitialized = false;

    public BlueEscapeState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
        this.escapeSpeed = controller.runSpeed * 1.2f; // Faster escape
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", true);
        controller.EnemyAnimator.SetBool("IsRunning", true);
        
        // Play escape sound
        if (controller.audioSource != null && controller.escapeSound != null)
        {
            controller.audioSource.PlayOneShot(controller.escapeSound);
        }
        
        // Get required components
        mouseBoid = controller.GetComponent<MouseBoid>();
        if (mouseBoid != null)
        {
            mouseBoid.SetMaxSpeed(escapeSpeed);
        }
        
        mouseMovement = controller.GetComponent<MouseMovement>();
        escapePathInitialized = false;
    }

    public override void Execute()
    {
        // Initialize escape path if not already done
        if (!escapePathInitialized && mouseMovement != null)
        {
           
            escapePathInitialized = true;
        }
        
        // Check if escape complete
        if (mouseMovement != null && mouseMovement.HasReachedEscapeTarget(controller.transform.position))
        {
            // If reached the last escape point, return to patrol
            if (mouseMovement.MoveToNextEscapePoint())
            {
                fsm.Transition(StateEnum.FlockingPatrolState);
                return;
            }
        }
        
        if (mouseBoid == null || mouseMovement == null) return;
        
        // Get target position from escape path
        Vector3 targetPos = mouseMovement.GetCurrentTargetPosition();
        Vector3 dirToTarget = (targetPos - controller.transform.position).normalized;
        
        // Add obstacle avoidance
        Vector3 avoidForce = controller.obstacleAvoidance.Avoid() * 2f;
        
        // Create escape force
        Vector3 escapeForce = dirToTarget * escapeSpeed;
        Vector3 combinedForce = escapeForce + avoidForce;
        
        // Apply forces with flocking behavior
        mouseBoid.SetPathFollowingForce(combinedForce);
        mouseBoid.ApplyFlocking();
        mouseBoid.UpdateMovement();
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsRunning", false);
        
        // Reset escape path
        if (mouseMovement != null)
        {
            
        }
    }
}