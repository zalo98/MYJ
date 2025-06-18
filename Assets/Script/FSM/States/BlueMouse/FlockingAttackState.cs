using UnityEngine;
using UnityEngine.SceneManagement;

public class FlockingAttackState : State
{
    private EnemyController enemyController;
    private EnemyVision enemyVision;
    private Transform playerTransform;
    private FlockingAttackDecisionTree attackDecisionTree;
    private MouseBoid mouseBoid;
    private FlockingManager flockingManager;

    public FlockingAttackState(EnemyController controller, FSM fsm) : base(fsm)
    {
        enemyController = controller;
        attackDecisionTree = new FlockingAttackDecisionTree(enemyController, fsm, this);
    }

    public override void Awake()
    {
        enemyVision = enemyController.GetComponent<EnemyVision>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        flockingManager = FlockingManager.Instance;
        
        mouseBoid = enemyController.GetComponent<MouseBoid>();
        if (mouseBoid != null)
        {
            mouseBoid.SetMaxSpeed(enemyController.runSpeed);
        }
        
        attackDecisionTree.StartAttack();
        enemyController.EnemyAnimator.SetBool("IsWalking", true);
        enemyController.EnemyAnimator.SetBool("IsRunning", true);
    }

    public override void Execute()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (enemyVision.HasDirectDetection)
        {
            BlueMouse blueMouse = enemyController as BlueMouse;
            if (blueMouse != null)
            {
                BlueMouse.BroadcastToAllMice(
                    StateEnum.FlockingAttackState,
                    blueMouse,
                    playerTransform.position
                );
            }
        }

        attackDecisionTree.Execute();

        if (IsCollidingWithPlayer())
        {
            Attack();
        }
    }

    public override void Sleep()
    {
        enemyController.EnemyAnimator.SetBool("IsRunning", false);
        attackDecisionTree.StopRotation();
    }

    private bool IsCollidingWithPlayer()
    {
        Collider playerCollider = playerTransform.GetComponent<Collider>();
        return enemyController.GetComponent<Collider>().bounds.Intersects(playerCollider.bounds);
    }

    private void Attack()
    {
        SceneManager.LoadScene("LoseScene");
    }
    
    public FlockingAttackDecisionTree GetDecisionTree()
    {
        return attackDecisionTree;
    }
}