using UnityEngine;

public class PatrolState : State
{
    private EnemyController controller;
    private float patrolDelay = 0f;
    private float timeSpentAtWaypoint = 0f;
    private int waypointCounter = 0;
    private int waypointLooking;

    public PatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        timeSpentAtWaypoint = 0f;
        waypointCounter = 0;
        waypointLooking = Mathf.RoundToInt(Randoms.RandomRange(3f, 5f));
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

        if (controller.mouseMovement.HasReachedCurrentTarget(controller.transform.position))
        {
            timeSpentAtWaypoint += Time.deltaTime;

            if (timeSpentAtWaypoint > patrolDelay)
            {
                waypointCounter++;
                
                if (waypointCounter >= waypointLooking)
                {
                    waypointCounter = 0;
                    controller.StateMachine.Transition(StateEnum.EnemyLookingState);
                    return;
                }
                else
                {
                    controller.mouseMovement.MoveToNextTarget();
                    timeSpentAtWaypoint = 0f;
                }
            }
        }

        controller.steering.MoveToPosition(controller.mouseMovement.GetCurrentTargetPosition(), controller.walkSpeed);
    }

    public override void Sleep()
    {
        AlertSystem.Instance.UnregisterEnemy(controller);
        controller.EnemyAnimator.SetBool("IsWalking", false);
    }
}