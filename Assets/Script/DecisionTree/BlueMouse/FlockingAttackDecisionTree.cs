using UnityEngine;

public class FlockingAttackDecisionTree
{
    private static bool anyBlueMiceSeesPlayer = false;
    private static Vector3? lastKnownPlayerPosition = null;
    private static bool flockIsRotating = false;
    private static float flockSearchTime = 5f;
    private static float rotationDuration = 5f;
    private static BlueMouse leaderMouse = null;
    private static bool isEscaping = false;
    private static float escapeTimer = 5f;

    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private readonly FlockingAttackState attackState;
    private IDecisionNode rootNode;
    private MouseBoid mouseBoid;
    private PlayerController playerController;
    private Flee fleeSteering;

    private float decisionInterval = 2f;
    private float decisionTimer = 0f;
    private float minAttackDistance = 0.01f;
    private float attackSpeed;
    private float escapeSpeed;

    public FlockingAttackDecisionTree(EnemyController enemy, FSM fsm, FlockingAttackState attackState)
    {
        this.enemy = enemy;
        this.fsm = fsm;
        this.attackState = attackState;
        this.attackSpeed = enemy.runSpeed;
        this.escapeSpeed = enemy.runSpeed * 1.2f;

        mouseBoid = enemy.GetComponent<MouseBoid>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            fleeSteering = new Flee(enemy.GetComponent<Rigidbody>(), playerObj.transform, escapeSpeed);
        }
        
        CreateTree();
    }

    public void StartAttack()
    {
        decisionTimer = 0f;
    }

    public void Execute()
    {
        if (playerController != null && playerController.isAttacking)
        {
            if (!isEscaping)
            {
                isEscaping = true;
                escapeTimer = 5f;
                
                BlueMouse blueMouse = enemy as BlueMouse;
                if (blueMouse != null)
                {
                    BlueMouse.BroadcastToAllMice(
                        StateEnum.BlueEscapeState, 
                        blueMouse,
                        enemy.PlayerTransform.position
                    );
                }
                
                fsm.Transition(StateEnum.BlueEscapeState);
            }
            return;
        }
        else if (isEscaping)
        {
            isEscaping = false;
        }

        BlueMouse currentMouse = enemy as BlueMouse;
        bool thisMouseSeesPlayer = enemy.enemyVision.HasDirectDetection || enemy.enemyVision.HasPeripheralDetection;

        if (thisMouseSeesPlayer)
        {
            anyBlueMiceSeesPlayer = true;
            lastKnownPlayerPosition = enemy.enemyVision.LastSeenPosition;

            if (flockIsRotating)
            {
                BroadcastRotation(false, enemy.enemyVision.LastSeenPosition);
            }

            if (currentMouse != null && enemy.PlayerTransform != null)
            {
                BlueMouse.BroadcastToAllMice(
                    StateEnum.FlockingAttackState,
                    currentMouse,
                    enemy.PlayerTransform.position
                );
            }
        }

        if (!thisMouseSeesPlayer && anyBlueMiceSeesPlayer)
        {
            bool anyOtherMouseSeesPlayer = false;
            foreach (var mouse in BlueMouse.GetAllBlueMice())
            {
                if (mouse != null && mouse != currentMouse &&
                    (mouse.enemyVision.HasDirectDetection || mouse.enemyVision.HasPeripheralDetection))
                {
                    anyOtherMouseSeesPlayer = true;
                    break;
                }
            }

            if (!anyOtherMouseSeesPlayer)
            {
                anyBlueMiceSeesPlayer = false;
                if (!flockIsRotating && leaderMouse == null)
                {
                    leaderMouse = currentMouse;
                }
                BroadcastRotation(true, lastKnownPlayerPosition);
            }
        }

        if (flockIsRotating)
        {
            if (leaderMouse == currentMouse)
            {
                if (flockSearchTime > 0f)
                {
                    flockSearchTime -= Time.deltaTime;

                    if (flockSearchTime <= 0f)
                    {
                        BroadcastRotation(false);
                        BroadcastPatrolTransition(currentMouse);
                        leaderMouse = null;
                    }
                }
            }

            if (flockSearchTime > 0f)
            {
                enemy.transform.Rotate(Vector3.up * 180f * Time.deltaTime);

                if (thisMouseSeesPlayer)
                {
                    BroadcastRotation(false, enemy.enemyVision.LastSeenPosition);
                    leaderMouse = null;

                    if (currentMouse != null && enemy.PlayerTransform != null)
                    {
                        BlueMouse.BroadcastToAllMice(
                            StateEnum.FlockingAttackState,
                            currentMouse,
                            enemy.PlayerTransform.position
                        );
                    }
                }
            }
        }
        else if (anyBlueMiceSeesPlayer && lastKnownPlayerPosition.HasValue)
        {
            Vector3 targetPosition = thisMouseSeesPlayer ?
                enemy.enemyVision.LastSeenPosition.Value :
                lastKnownPlayerPosition.Value;

            MoveToPositionWithFlocking(targetPosition);
        }
        else if (!anyBlueMiceSeesPlayer && !flockIsRotating)
        {
            if (leaderMouse == null)
            {
                leaderMouse = currentMouse;
            }
            BroadcastRotation(true, lastKnownPlayerPosition);
        }
    }

    private void MoveToPositionWithFlocking(Vector3 targetPos)
    {
        if (mouseBoid == null) return;
        
        Vector3 dirToTarget = (targetPos - enemy.transform.position).normalized;
        float distToTarget = Vector3.Distance(enemy.transform.position, targetPos);
        
        Vector3 moveForce = dirToTarget * attackSpeed;
        Vector3 avoidForce = enemy.obstacleAvoidance.Avoid() * 1.5f;
        Vector3 combinedForce = moveForce + avoidForce;
        
        mouseBoid.SetPathFollowingForce(combinedForce);
        mouseBoid.ApplyFlocking();
        mouseBoid.UpdateMovement();
    }

    private void BroadcastPatrolTransition(BlueMouse initiatorMouse)
    {
        foreach (var mouse in BlueMouse.GetAllBlueMice())
        {
            if (mouse != null && mouse != initiatorMouse)
            {
                mouse.TransitionToState(StateEnum.FlockingPatrolState);
            }
        }
        
        fsm.Transition(StateEnum.FlockingPatrolState);
    }

    private void BroadcastRotation(bool startRotation, Vector3? lastPosition = null)
    {
        flockIsRotating = startRotation;
        
        if (startRotation)
        {
            flockSearchTime = rotationDuration;
        }
        else
        {
            flockSearchTime = 0f;
        }
    }

    private void CreateTree()
    {
        if (fsm == null || enemy == null) return;

        ActionNode escapar = new ActionNode(() => {
            if (playerController != null && playerController.isAttacking)
            {
                fsm.Transition(StateEnum.BlueEscapeState);
            }
        });

        ActionNode atacar = new ActionNode(() => {
            if (enemy.enemyVision.LastSeenPosition.HasValue)
            {
                Vector3 targetPosition = enemy.enemyVision.LastSeenPosition.Value;
                MoveToPositionWithFlocking(targetPosition);

                if (flockIsRotating)
                {
                    BroadcastRotation(false);
                }
            }
        });

        ActionNode patrullar = new ActionNode(() => {
            BlueMouse blueMouse = enemy as BlueMouse;
            if (blueMouse != null)
            {
                BroadcastPatrolTransition(blueMouse);
            }
        });

        QuestionNode estaGirando = new QuestionNode(atacar, patrullar, () => flockSearchTime > 0f);
        QuestionNode jugadorVisible = new QuestionNode(atacar, estaGirando, () => anyBlueMiceSeesPlayer || enemy.enemyVision.HasDirectDetection);
        
        rootNode = new QuestionNode(escapar, jugadorVisible, () => playerController.isAttacking);
    }

    public void StopRotation()
    {
        if (flockIsRotating)
        {
            BroadcastRotation(false);
            leaderMouse = null;
        }
    }
}
