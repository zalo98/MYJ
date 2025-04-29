using UnityEngine;

public class AlertDecisionTree
{
    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private IDecisionNode rootNode;

    public AlertDecisionTree(EnemyController enemy, FSM fsm)
    {
        this.enemy = enemy;
        this.fsm = fsm;
    }

    public void StartAlert()
    {
        CreateTree();
    }

    public void Execute()
    {
        rootNode?.Execute();
    }

    public void CreateTree()
    {
        if (fsm == null || enemy == null)
        {
            Debug.LogError("FSM or EnemyController is null.");
            return;
        }

        ActionNode atacar = new ActionNode(() => fsm.Transition(StateEnum.Attack));
        ActionNode buscar = new ActionNode(() => AlertLookOrMove());

        ActionNode patrullar = new ActionNode(() => fsm.Transition(StateEnum.EnemyPatrol));

        QuestionNode veAlJugador = new QuestionNode(atacar, buscar, () => enemy.enemyVision.HasDirectDetection);
        QuestionNode hayUltimaPosicion = new QuestionNode(veAlJugador, patrullar, () => enemy.enemyVision.LastSeenPosition.HasValue);

        rootNode = hayUltimaPosicion;

        Debug.Log("Decision tree created successfully.");
    }

    private void AlertLookOrMove()
    {
        if (Randoms.Chance(0.5f))
        {
            Debug.Log("Enemigo mirando a sus alrededores");
        }
        else if (enemy.enemyVision.LastSeenPosition.HasValue)
        {
            Vector3 target = enemy.enemyVision.LastSeenPosition.Value;
            enemy.Steering.MoveToPosition(target, enemy.walkSpeed);
            Debug.Log("Enemigo se mueve hacia la última posición del jugador");
        }
    }
}