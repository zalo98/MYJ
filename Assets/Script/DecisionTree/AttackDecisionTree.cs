using UnityEngine;

public class AttackDecisionTree
{
    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private readonly AttackState attackState;
    private IDecisionNode rootNode;

    private float decisionInterval = 2f;
    private float decisionTimer = 0f;
    private bool isRotating = false;
    private float searchTimer = 5f;
    private float currentSearchTime;

    public AttackDecisionTree(EnemyController enemy, FSM fsm, AttackState attackState)
    {
        this.enemy = enemy;
        this.fsm = fsm;
        this.attackState = attackState;
    }

    public void StartAttack()
    {
        CreateTree();
        decisionTimer = 0f;
        isRotating = false;
        currentSearchTime = searchTimer;
    }

    public void Execute()
    {
        if (enemy.enemyVision.HasDirectDetection)
        {
            enemy.Steering.MoveToPosition(enemy.enemyVision.LastSeenPosition.Value, enemy.runSpeed);
            currentSearchTime = searchTimer;
            isRotating = false;
            return;
        }

        if (isRotating)
        {
            enemy.transform.Rotate(Vector3.up * 180f * Time.deltaTime);
            
            currentSearchTime -= Time.deltaTime;
            if (currentSearchTime <= 0f)
            {
                fsm.Transition(StateEnum.EnemyPatrol);
                return;
            }
            return;
        }
        
        isRotating = true;
        Debug.Log("Enemigo empieza a buscar al jugador");
    }

    private void CreateTree()
    {
        if (fsm == null || enemy == null)
        {
            Debug.LogError("FSM or EnemyController is null.");
            return;
        }
        
        ActionNode atacar = new ActionNode(() => {
            enemy.Steering.MoveToPosition(enemy.enemyVision.LastSeenPosition.Value, enemy.runSpeed);
            currentSearchTime = searchTimer;
            isRotating = false;
        });
        
        ActionNode buscar = new ActionNode(() => {
            isRotating = true;
            Debug.Log("Enemigo empieza a buscar al jugador");
        });
        
        ActionNode patrullar = new ActionNode(() => fsm.Transition(StateEnum.EnemyPatrol));
        
        QuestionNode tiempoAgotado = new QuestionNode(patrullar, buscar, () => currentSearchTime <= 0f);
        QuestionNode estaGirando = new QuestionNode(tiempoAgotado, buscar, () => isRotating);
        
        rootNode = new QuestionNode(atacar, estaGirando, () => enemy.enemyVision.HasDirectDetection);
    }

    public void StopRotation()
    {
        if (isRotating)
        {
            isRotating = false;
            Debug.Log("Enemigo deja de buscar");
        }
    }
}
