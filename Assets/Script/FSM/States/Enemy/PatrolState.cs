using UnityEngine;

public class PatrolState : State
{
    private EnemyController controller;
    private float patrolDelay = 0f;
    private float timeSpentAtWaypoint = 0f;

    public PatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        timeSpentAtWaypoint = 0f;
        AlertSystem.Instance.RegisterEnemy(controller);
        controller.EnemyAnimator.SetBool("IsWalking", true);
        Debug.Log("Enemigo entró a PatrolState");
    }

    public override void Execute()
    {
        if (controller.enemyVision.HasDirectDetection)
        {
            controller.StateMachine.Transition(StateEnum.Attack);
            return;
        }
        else if (controller.enemyVision.usePeripheralVision && controller.enemyVision.HasPeripheralDetection)
        {
            controller.StateMachine.Transition(StateEnum.EnemyAlert);
            return;
        }

        if (controller.WaypointSystem.HasReachedCurrentTarget(controller.transform.position))
        {
            timeSpentAtWaypoint += Time.deltaTime;

            if (timeSpentAtWaypoint > patrolDelay)
            {
                controller.StateMachine.Transition(StateEnum.EnemyLookingState);
                return;
            }
        }

        controller.steering.MoveToPosition(controller.WaypointSystem.GetCurrentTargetPosition(), controller.walkSpeed);
    }

    public override void Sleep()
    {
        AlertSystem.Instance.UnregisterEnemy(controller);
        controller.EnemyAnimator.SetBool("IsWalking", false);
    }
}