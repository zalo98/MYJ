using UnityEngine;

public class AlertDecisionTree
{
    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private readonly AlertState alertState;
    private IDecisionNode rootNode;

    private float decisionInterval = 2f;
    private float decisionTimer = 0f;
    private float alertTimer = 10f;
    private float currentTime;

    private bool isRotating = false;
    public Vector3? currentTarget = null;
    public bool wasAlertedByCamera = false;
    public Vector3? cameraAlertPosition = null;

    public AlertDecisionTree(EnemyController enemy, FSM fsm, AlertState alertState)
    {
        this.enemy = enemy;
        this.fsm = fsm;
        this.alertState = alertState;
    }

    public void StartAlert()
    {
        CreateTree();
        decisionTimer = 0f;
        isRotating = false;
        currentTarget = null;
        wasAlertedByCamera = false;
        cameraAlertPosition = null;
        currentTime = alertTimer;
    }

    public void AlertByCamera(Vector3 position)
    {
        wasAlertedByCamera = true;
        cameraAlertPosition = position;
        currentTarget = position;
        currentTime = alertTimer;
        Debug.Log($"Enemigo alertado por cámara en posición: {position}");
    }

    public void Execute()
    {
        if (enemy.enemyVision.HasDirectDetection)
        {
            rootNode?.Execute();
            return;
        }

        if (isRotating)
        {
            enemy.transform.Rotate(Vector3.up * 180f * Time.deltaTime);
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0f)
            {
                fsm.Transition(StateEnum.EnemyPatrol);
                return;
            }
            return;
        }

        if (currentTarget.HasValue)
        {
            if (enemy.enemyVision.HasDirectDetection)
            {
                rootNode?.Execute();
                return;
            }
            else if (enemy.enemyVision.HasPeripheralDetection)
            {
                AlertLookOrMove();
                return;
            }

            // Actualizar la posición del targetTransform y usar MoveToPosition
            enemy.steering.MoveToPosition(currentTarget.Value, enemy.walkSpeed);

            currentTime = alertTimer;

            float distance = Vector3.Distance(enemy.transform.position, currentTarget.Value);
            if (distance <= 1f)
            {
                currentTarget = null;
                Debug.Log("Llegó a la última posición del jugador");
                isRotating = true;
                Debug.Log("Enemigo empieza a rotar alrededor de la posición");
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
        ActionNode irAPosicionCamara = new ActionNode(() => MoveToCameraPosition());
        ActionNode buscar = new ActionNode(() => AlertLookOrMove());
        ActionNode patrullar = new ActionNode(() => fsm.Transition(StateEnum.EnemyPatrol));
        
        QuestionNode tiempoAgotado = new QuestionNode(patrullar, buscar, () => currentTime <= 0f);
        QuestionNode hayUltimaPosicion = new QuestionNode(tiempoAgotado, patrullar, () => enemy.enemyVision.LastSeenPosition.HasValue);
        
        QuestionNode fueAlertadoPorCamara = new QuestionNode(
            irAPosicionCamara,
            hayUltimaPosicion,
            () => wasAlertedByCamera
        );
        
        rootNode = new QuestionNode(atacar, fueAlertadoPorCamara, () => enemy.enemyVision.HasDirectDetection);
    }

    private void MoveToCameraPosition()
    {
        if (!cameraAlertPosition.HasValue)
        {
            return;
        }
        
        if (enemy.enemyVision.HasDirectDetection)
        {
            rootNode?.Execute();
            return;
        }
        else if (enemy.enemyVision.HasPeripheralDetection)
        {
            AlertLookOrMove();
            return;
        }

        Vector3 target = cameraAlertPosition.Value;
        if (Vector3.Distance(enemy.transform.position, target) > 1f)
        {
            currentTarget = target;
            enemy.steering.MoveToPosition(target, enemy.walkSpeed);
            currentTime = alertTimer;
            Debug.Log("Enemigo se mueve hacia la posición marcada por la cámara");
        }
        else
        {
            Debug.Log("Ya está en la posición marcada por la cámara");
            isRotating = true;
        }
    }

    private void AlertLookOrMove()
    {
        if (!enemy.enemyVision.LastSeenPosition.HasValue)
            return;

        // Verificar visión antes de decidir qué hacer
        if (enemy.enemyVision.HasDirectDetection)
        {
            rootNode?.Execute();
            return;
        }

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
                enemy.steering.MoveToPosition(target, enemy.walkSpeed);
                currentTime = alertTimer;
                Debug.Log("Enemigo se mueve hacia la última posición del jugador");
            }
            else
            {
                Debug.Log("Última posición demasiado cercana, enemigo no se mueve.");
                isRotating = true;
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