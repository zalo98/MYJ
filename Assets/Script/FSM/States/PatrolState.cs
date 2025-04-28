using UnityEngine;

public class PatrolState : State
{
    private EnemyController enemyController;
    private EnemyVision enemyVision;

    public PatrolState(EnemyController controller, FSM fsm) : base(fsm)
    {
        enemyController = controller;
    }

    public override void Awake()
    {
        enemyController.WaypointSystem.ResetToStart();
        enemyVision = enemyController.GetComponent<EnemyVision>();
    }

    public override void Execute()
    {
        Vector3 targetPosition = enemyController.WaypointSystem.GetCurrentTargetPosition();
        MoveTowardsTarget(targetPosition);
    
        if (enemyController.WaypointSystem.HasReachedCurrentTarget(enemyController.transform.position))
        {
            enemyController.WaypointSystem.MoveToNextTarget();
        }
        
        if (enemyVision.HasDirectDetection)
        {
            enemyController.StateMachine.Transition(StateEnum.Attack);
        }
        else if (enemyController.enemyVision.usePeripheralVision && enemyController.enemyVision.HasPeripheralDetection)
        {
            enemyController.StateMachine.Transition(StateEnum.EnemyAlert);
        }
    }

    private void MoveTowardsTarget(Vector3 target)
    {
        Vector3 direction = (target - enemyController.transform.position);
        direction.y = 0f;
        Vector3 normalizedDirection = direction.normalized;

        if (normalizedDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(normalizedDirection);
        
            enemyController.transform.rotation = Quaternion.RotateTowards(
                enemyController.transform.rotation,
                targetRotation,
                enemyController.rotationSpeed * Time.deltaTime
            );
        }

        float speed = enemyController.walkSpeed;

        if (enemyController.WaypointSystem.HasReachedCurrentTarget(enemyController.transform.position))
        {
            speed = enemyController.runSpeed;
        }

        // >>>> 🔵 Agregado de obstacle avoidance 🔵 <<<<
        Vector3 avoidance = enemyController.obstacleAvoidance.Avoid();
        Vector3 finalDirection = (normalizedDirection + avoidance).normalized;

        enemyController.transform.position += finalDirection * speed * Time.deltaTime;
    }
}