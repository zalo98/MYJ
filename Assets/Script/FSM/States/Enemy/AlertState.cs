using UnityEngine;

public class AlertState : State
{
    private EnemyController controller;
    private AlertDecisionTree decisionTree;
    private Transform playerTransform;
    private EnemyVision enemyVision;

    public AlertState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
    }

    public override void Awake()
    {
        Debug.Log("Entering AlertState");
        
        enemyVision = controller.GetComponent<EnemyVision>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("No se ha encontrado el transform del jugador.");
            return;
        }

        decisionTree = new AlertDecisionTree(controller, fsm);
        decisionTree.StartAlert();
    }

    public override void Execute()
    {
        decisionTree.Execute();
    }

    public override void Sleep()
    {
        Debug.Log("Exiting AlertState");

        decisionTree = null;
    }
}