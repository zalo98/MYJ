using UnityEngine;

public class AlertState : State
{
    private EnemyController controller;
    public AlertDecisionTree decisionTree;
    private Transform playerTransform;
    private EnemyVision enemyVision;

    public AlertState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        Debug.Log("Entering AlertState");
        controller.EnemyAnimator.SetBool("IsLooking", true);

        enemyVision = controller.GetComponent<EnemyVision>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("No se ha encontrado el transform del jugador.");
            return;
        }

        decisionTree = new AlertDecisionTree(controller, fsm, this);
        decisionTree.StartAlert();
    }

    public override void Execute()
    {
        if (enemyVision.HasPeripheralDetection)
        {
            Debug.Log("Jugador visto periféricamente");
        }
        
        if (decisionTree.currentTarget.HasValue)
        {
            Debug.Log("Enemigo tiene un objetivo al que moverse");
        }
    
        decisionTree.Execute();
    }

    public override void Sleep()
    {
        Debug.Log("Exiting AlertState");
        controller.EnemyAnimator.SetBool("IsLooking", false);
        decisionTree = null;
    }
}