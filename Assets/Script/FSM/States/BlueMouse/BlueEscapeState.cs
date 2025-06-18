using UnityEngine;

public class BlueEscapeState : State
{
    private EnemyController controller;
    private BlueMouse blueMouse;
    private MouseBoid mouseBoid;
    private Flee fleeSteering;
    private float escapeSpeed;
    private float escapeTimer = 5f;
    private static bool isEscaping = false;
    private static BlueMouse escapeInitiator = null;
    private Transform playerTransform;
    private Rigidbody rb;

    public BlueEscapeState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
        this.escapeSpeed = controller.runSpeed * 1.2f;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", true);
        controller.EnemyAnimator.SetBool("IsRunning", true);
        
        BlueMouse blueMouse = controller as BlueMouse;
        blueMouse.ChangeEscapeBool();
        
        escapeTimer = 5f;
        
        if (controller.audioSource != null && controller.escapeSound != null)
        {
            controller.audioSource.PlayOneShot(controller.escapeSound);
        }
        
        mouseBoid = controller.GetComponent<MouseBoid>();
        if (mouseBoid != null)
        {
            mouseBoid.SetMaxSpeed(escapeSpeed);
        }
        
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        rb = controller.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = controller.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        fleeSteering = new Flee(rb, playerTransform, escapeSpeed);
        
        if (!isEscaping && blueMouse != null)
        {
            isEscaping = true;
            escapeInitiator = blueMouse;
            
            BlueMouse.BroadcastToAllMice(
                StateEnum.BlueEscapeState,
                blueMouse,
                playerTransform.position
            );
        }
    }

    public override void Execute()
    {
        escapeTimer -= Time.deltaTime;
        
        if (escapeTimer <= 0f)
        {
            BlueMouse blueMouse = controller as BlueMouse;
            if (blueMouse == escapeInitiator)
            {
                isEscaping = false;
                escapeInitiator = null;
                
                foreach (var mouse in BlueMouse.GetAllBlueMice())
                {
                    if (mouse != null && mouse != blueMouse)
                    {
                        mouse.TransitionToState(StateEnum.FlockingPatrolState);
                    }
                }
            }
            
            fsm.Transition(StateEnum.FlockingPatrolState);
            return;
        }
        
        if (playerTransform == null || fleeSteering == null) return;
        
        Vector3 fleeDirection = fleeSteering.MoveDirection();
        Vector3 avoidForce = controller.obstacleAvoidance.Avoid() * 2f;
        Vector3 combinedForce = fleeDirection + avoidForce;
        
        if (mouseBoid != null)
        {
            controller.transform.position += combinedForce * Time.deltaTime;
            
            if (combinedForce != Vector3.zero)
            {
                controller.transform.forward = combinedForce.normalized;
            }
        }
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsRunning", false);
        BlueMouse blueMouse = controller as BlueMouse;
        blueMouse.ChangeEscapeBool();
        
        if (blueMouse == escapeInitiator)
        {
            isEscaping = false;
            escapeInitiator = null;
        }
    }
}

