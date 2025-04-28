using UnityEngine;

public class AttackState : State
{
    private EnemyController enemyController;
    private EnemyVision enemyVision;
    private Transform playerTransform;  // Referencia al Transform del jugador

    public AttackState(EnemyController controller, FSM fsm) : base(fsm)
    {
        enemyController = controller;
    }

    public override void Awake()
    {
        enemyVision = enemyController.GetComponent<EnemyVision>();
        // Obtener el Transform del jugador
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override void Execute()
    {
        if (playerTransform == null)
        {
            Debug.LogError("No se ha encontrado el jugador.");
            return;
        }

        // Mover hacia el jugador
        MoveTowardsPlayer();

        // Comprobar si el enemigo está colisionando con el jugador
        if (IsCollidingWithPlayer())
        {
            // Ataque: realizar el daño al jugador al colisionar
            Attack();
        }

        // Si el enemigo ya no ve al jugador, volver al estado de patrullaje
        if (!enemyVision.HasDirectDetection)
        {
            enemyController.StateMachine.Transition(StateEnum.EnemyPatrol);
        }
    }

    private void MoveTowardsPlayer()
    {
        // Mueve al enemigo hacia el jugador
        Vector3 playerPosition = playerTransform.position;
        Vector3 direction = (playerPosition - enemyController.transform.position).normalized;
        enemyController.transform.position += direction * enemyController.runSpeed * Time.deltaTime;
    }

    private bool IsCollidingWithPlayer()
    {
        // Aquí puedes hacer una verificación simple de colisión con el jugador
        // Suponiendo que el jugador tiene un tag "Player"
        Collider playerCollider = playerTransform.GetComponent<Collider>();
        
        return enemyController.GetComponent<Collider>().bounds.Intersects(playerCollider.bounds);
    }

    private void Attack()
    {
        // El enemigo realiza su "ataque" al chocar con el jugador
        Debug.Log("Enemigo ha atacado al jugador!");

        // Aquí puedes agregar la lógica para aplicar daño al jugador
        // Ejemplo: player.TakeDamage(damageAmount);
        
        // Después de atacar, volvemos al estado de patrullaje
        enemyController.StateMachine.Transition(StateEnum.EnemyPatrol);
    }
}