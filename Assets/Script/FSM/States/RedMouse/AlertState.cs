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
        controller.EnemyAnimator.SetBool("IsLooking", true);

        enemyVision = controller.GetComponent<EnemyVision>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            return;
        }

        decisionTree = new AlertDecisionTree(controller, fsm, this);
        decisionTree.StartAlert();
    }

    public override void Execute()
    {
        decisionTree.Execute();
    }

    public override void Sleep()
    {
        controller.EnemyAnimator.SetBool("IsLooking", false);
        decisionTree = null;
    }
}