using UnityEngine;

public class PatrolState : State
{
    private EnemyController controller;
    private float patrolDelay = 1f;
    private float timeSpentAtWaypoint = 0f;

    public PatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        timeSpentAtWaypoint = 0f;
        AlertSystem.Instance.RegisterEnemy(controller);
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
                controller.WaypointSystem.MoveToNextTarget();
                timeSpentAtWaypoint = 0f;
            }
        }

        MoveTowardsTarget(controller.WaypointSystem.GetCurrentTargetPosition(), controller.walkSpeed);
    }

    public override void Sleep()
    {
        AlertSystem.Instance.UnregisterEnemy(controller);
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

            Vector3 avoidance = controller.obstacleAvoidance.Avoid();
            Vector3 finalDirection = (direction.normalized + avoidance).normalized;

            controller.transform.position += finalDirection * speed * Time.deltaTime;
        }
    }
}