using UnityEngine;
using UnityEngine.SceneManagement;

public class AttackState : State
{
    private EnemyController enemyController;
    private EnemyVision enemyVision;
    private Transform playerTransform;
    private AttackDecisionTree attackDecisionTree;

    public AttackState(EnemyController controller, FSM fsm) : base(fsm)
    {
        enemyController = controller;
        attackDecisionTree = new AttackDecisionTree(enemyController, fsm, this);
    }

    public override void Awake()
    {
        enemyVision = enemyController.GetComponent<EnemyVision>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        attackDecisionTree.StartAttack();
    }

    public override void Execute()
    {
        if (playerTransform == null)
        {
            Debug.LogError("No se ha encontrado el jugador.");
            return;
        }
        
        attackDecisionTree.Execute();
        
        if (IsCollidingWithPlayer())
        {
            Attack();
        }
    }

    private bool IsCollidingWithPlayer()
    {
        Collider playerCollider = playerTransform.GetComponent<Collider>();
        return enemyController.GetComponent<Collider>().bounds.Intersects(playerCollider.bounds);
    }

    private void Attack()
    {
        Debug.Log("Enemigo ha atacado al jugador!");
        SceneManager.LoadScene("LoseScene");
    }
}