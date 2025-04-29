using UnityEngine;

public class AlertDecisionTree
{
    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private readonly AlertState alertState;
    private IDecisionNode rootNode;

    private float decisionInterval = 2f;
    private float decisionTimer = 0f;

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
    }

    public void AlertByCamera(Vector3 position)
    {
        wasAlertedByCamera = true;
        cameraAlertPosition = position;
        currentTarget = position;
        alertState.alertTimer = 10f;
        alertState.currentTime = 10f;
        Debug.Log($"Enemigo alertado por cámara en posición: {position}");
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
            
            alertState.currentTime -= Time.deltaTime;
            if (alertState.currentTime <= 0f)
            {
                fsm.Transition(StateEnum.EnemyPatrol);
                return;
            }
            return;
        }

        if (currentTarget.HasValue)
        {
            enemy.Steering.MoveToPosition(currentTarget.Value, enemy.walkSpeed);
            
            alertState.currentTime = alertState.alertTimer;

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

        alertState.currentTime -= Time.deltaTime;
        if (alertState.currentTime <= 0f)
        {
            fsm.Transition(StateEnum.EnemyPatrol);
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
        
        QuestionNode tiempoAgotado = new QuestionNode(patrullar, buscar, () => alertState.currentTime <= 0f);
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

        Vector3 target = cameraAlertPosition.Value;
        if (Vector3.Distance(enemy.transform.position, target) > 1f)
        {
            currentTarget = target;
            enemy.Steering.MoveToPosition(target, enemy.walkSpeed);
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