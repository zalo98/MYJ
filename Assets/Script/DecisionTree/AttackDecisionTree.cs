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
        if (enemy.enemyVision.HasDirectDetection || enemy.enemyVision.HasPeripheralDetection)
        {
            enemy.Steering.MoveToPosition(enemy.enemyVision.LastSeenPosition.Value, enemy.runSpeed);
            currentSearchTime = searchTimer;
            isRotating = false;
            return;
        }

        if (isRotating)
        {
            if (currentSearchTime > 0f)
            {
                enemy.transform.Rotate(Vector3.up * 180f * Time.deltaTime);
                currentSearchTime -= Time.deltaTime;
                Debug.Log($"Tiempo restante: {currentSearchTime}");
            }
            
            if (rootNode != null)
            {
                rootNode.Execute();
            }
            return;
        }
        
        if (enemy.enemyVision.LastSeenPosition.HasValue)
        {
            Vector3 lastPosition = enemy.enemyVision.LastSeenPosition.Value;
            float distance = Vector3.Distance(enemy.transform.position, lastPosition);
            
            if (distance > 1f)
            {
                enemy.Steering.MoveToPosition(lastPosition, enemy.runSpeed);
            }
            else
            {
                isRotating = true;
                Debug.Log("Enemigo empieza a buscar al jugador");
            }
        }
        else
        {
            isRotating = true;
            Debug.Log("Enemigo empieza a buscar al jugador");
        }
    }

    private void CreateTree()
    {
        if (fsm == null || enemy == null)
        {
            Debug.LogError("FSM or EnemyController is null.");
            return;
        }
        
        ActionNode atacar = new ActionNode(() => { 
            if (enemy.enemyVision.LastSeenPosition.HasValue)
            {
                enemy.Steering.MoveToPosition(enemy.enemyVision.LastSeenPosition.Value, enemy.runSpeed); 
                currentSearchTime = searchTimer;
                isRotating = false;
            }
        });
        
        ActionNode patrullar = new ActionNode(() => { fsm.Transition(StateEnum.EnemyPatrol); isRotating = false; });
        
        QuestionNode estaGirando = new QuestionNode(atacar, patrullar, () => currentSearchTime > 0f);
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
