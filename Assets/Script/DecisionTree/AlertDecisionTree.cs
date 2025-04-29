using UnityEngine;

public class AlertDecisionTree
{
    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private IDecisionNode rootNode;

    private float decisionInterval = 2f;
    private float decisionTimer = 0f;

    private bool isRotating = false;
    private Vector3? currentTarget = null;

    public AlertDecisionTree(EnemyController enemy, FSM fsm)
    {
        this.enemy = enemy;
        this.fsm = fsm;
    }

    public void StartAlert()
    {
        CreateTree();
        decisionTimer = 0f;
        isRotating = false;
        currentTarget = null;
    }

    public void Execute()
    {
        if (enemy.enemyVision.HasDirectDetection)
        {
            fsm.Transition(StateEnum.Attack);
            return;
        }

        if (isRotating)
        {
            enemy.transform.Rotate(Vector3.up * 180f * Time.deltaTime);
            return;
        }

        if (currentTarget.HasValue)
        {
            enemy.Steering.MoveToPosition(currentTarget.Value, enemy.walkSpeed);

            float distance = Vector3.Distance(enemy.transform.position, currentTarget.Value);
            if (distance <= 1f)
            {
                currentTarget = null;
                Debug.Log("Llegó a la última posición del jugador");
            }
            return;
        }

        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionInterval;
            rootNode?.Execute();
        }
    }

    private void CreateTree()
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
    }

    private void AlertLookOrMove()
    {
        if (!enemy.enemyVision.LastSeenPosition.HasValue)
            return;

        if (Randoms.Chance(0.5f))
        {
            isRotating = true;
            Debug.Log("Enemigo empieza a mirar alrededor");
        }
        else
        {
            Vector3 target = enemy.enemyVision.LastSeenPosition.Value;

            if (Vector3.Distance(enemy.transform.position, target) > 1f)
            {
                currentTarget = target;
                enemy.Steering.MoveToPosition(target, enemy.walkSpeed);
                Debug.Log("Enemigo se mueve hacia la última posición del jugador");
            }
            else
            {
                Debug.Log("Última posición demasiado cercana, enemigo no se mueve.");
            }
        }
    }

    public void StopRotation()
    {
        if (isRotating)
        {
            isRotating = false;
            Debug.Log("Enemigo deja de rotar");
        }
    }
}