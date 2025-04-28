using UnityEngine;

public class MousePatrolState : State
{
    private EnemyController controller;
    private float patrolDelay = 1f;
    private float timeSpentAtWaypoint = 0f;
    private int patrolCount = 0;
    private int maxPatrolCount = 2;

    public MousePatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", true);
    }

    public override void Execute()
    {
        if (controller.enemyVision.HasDirectDetection)
        {
            controller.StateMachine.Transition(StateEnum.MouseEscapeState);
            return;
        }
        
        if (controller.WaypointSystem.HasReachedCurrentTarget(controller.transform.position))
        {
            timeSpentAtWaypoint += Time.deltaTime;
            if (timeSpentAtWaypoint > patrolDelay)
            {
                patrolCount++;
                
                if (patrolCount >= maxPatrolCount)
                {
                    controller.StateMachine.Transition(StateEnum.MouseLookingState);
                    return;
                }
                
                controller.WaypointSystem.MoveToNextTarget();
                timeSpentAtWaypoint = 0f;
            }
        }
        
        Vector3 targetPosition = controller.WaypointSystem.GetCurrentTargetPosition();
        MoveTowardsTarget(targetPosition, controller.walkSpeed);
    }

    private void MoveTowardsTarget(Vector3 target, float speed)
    {
        Vector3 direction = (target - controller.transform.position);
        direction.y = 0f; 
        
        if (direction.sqrMagnitude > 0.1f)
        {
            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation,
                Quaternion.LookRotation(direction),
                controller.rotationSpeed * Time.deltaTime
            );
            
            controller.transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsWalking", false);
    }
}