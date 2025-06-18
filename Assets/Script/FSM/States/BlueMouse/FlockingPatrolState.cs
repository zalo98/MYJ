using UnityEngine;

public class FlockingPatrolState : State
{
    private EnemyController controller;
    private MouseBoid mouseBoid;
    private FlockingManager flockingManager;
    private float obstacleAvoidanceWeight = 1.5f;

    public FlockingPatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", true);
        controller.EnemyAnimator.SetBool("IsRunning", false);
        
        mouseBoid = controller.GetComponent<MouseBoid>();
        if (mouseBoid == null)
        {
            mouseBoid = controller.gameObject.AddComponent<MouseBoid>();
        }
        mouseBoid.SetMaxSpeed(controller.walkSpeed);

        flockingManager = FlockingManager.Instance;
        if (flockingManager != null)
        {
            flockingManager.AddBoid(mouseBoid);
        }
    }

    public override void Execute()
    {
        if (controller.enemyVision.HasDirectDetection)
        {
            BlueMouse blueMouse = controller as BlueMouse;
            if (blueMouse != null)
            {
                BlueMouse.BroadcastToAllMice(
                    StateEnum.FlockingAttackState,
                    blueMouse,
                    controller.PlayerTransform.position
                );
            }

            fsm.Transition(StateEnum.FlockingAttackState);
            return;
        }

        if (flockingManager == null)
        {
            flockingManager = FlockingManager.Instance;
            if (flockingManager == null) return;
        }

        if (mouseBoid == null)
        {
            mouseBoid = controller.GetComponent<MouseBoid>();
            if (mouseBoid == null) return;
        }

        Vector3 avoidForce = controller.obstacleAvoidance.Avoid() * obstacleAvoidanceWeight;
        Vector3 pathForce = flockingManager.GetPathFollowingForce(controller.transform.position, mouseBoid._maxSpeed);
        Vector3 combinedForce = pathForce + avoidForce;

        mouseBoid.SetPathFollowingForce(combinedForce);
        mouseBoid.ApplyFlocking();
        mouseBoid.UpdateMovement();
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsWalking", false);
    }
}