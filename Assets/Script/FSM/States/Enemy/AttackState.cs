using UnityEngine;

public class AttackState : State
{
    private EnemyController enemyController;
    private EnemyVision enemyVision;
    private Transform playerTransform;

    public AttackState(EnemyController controller, FSM fsm) : base(fsm)
    {
        enemyController = controller;
    }

    public override void Awake()
    {
        enemyVision = enemyController.GetComponent<EnemyVision>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void Execute()
    {
        if (playerTransform == null)
        {
            Debug.LogError("No se ha encontrado el jugador.");
            return;
        }
        
        MoveTowardsPlayer();
        
        if (IsCollidingWithPlayer())
        {
            Attack();
        }
        
        if (!enemyVision.HasDirectDetection)
        {
            enemyController.StateMachine.Transition(StateEnum.EnemyPatrol);
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 playerPosition = playerTransform.position;
        Vector3 direction = (playerPosition - enemyController.transform.position).normalized;
        enemyController.transform.position += direction * enemyController.runSpeed * Time.deltaTime;
    }

    private bool IsCollidingWithPlayer()
    {
        Collider playerCollider = playerTransform.GetComponent<Collider>();
        
        return enemyController.GetComponent<Collider>().bounds.Intersects(playerCollider.bounds);
    }

    private void Attack()
    {
        Debug.Log("Enemigo ha atacado al jugador!");
        enemyController.StateMachine.Transition(StateEnum.EnemyPatrol);
    }
}