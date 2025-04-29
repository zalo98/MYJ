using UnityEngine;

public class AlertState : State
{
    private EnemyController controller;
    private AlertDecisionTree decisionTree;
    private Transform playerTransform;
    private EnemyVision enemyVision;

    private float alertTimer = 10f;
    private float currentTime;

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

        currentTime = alertTimer;
    }

    public override void Execute()
    {
        if (enemyVision.HasDirectDetection)
        {
            fsm.Transition(StateEnum.Attack);
            return;
        }

        if (enemyVision.HasPeripheralDetection)
        {
            currentTime = alertTimer;
            Debug.Log("Jugador visto periféricamente, timer reiniciado");
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            fsm.Transition(StateEnum.EnemyPatrol);
            return;
        }

        decisionTree.Execute();
    }

    public override void Sleep()
    {
        Debug.Log("Exiting AlertState");
        decisionTree = null;
    }
}