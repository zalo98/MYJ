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
        
        if (controller.mouseMovement.HasReachedCurrentTarget(controller.transform.position))
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
                
                controller.mouseMovement.MoveToNextTarget();
                timeSpentAtWaypoint = 0f;
            }
        }
        
        controller.steering.MoveToPosition(controller.mouseMovement.GetCurrentTargetPosition(), controller.walkSpeed);
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsWalking", false);
    }
}