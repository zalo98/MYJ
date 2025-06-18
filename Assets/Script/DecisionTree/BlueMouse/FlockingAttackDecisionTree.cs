using UnityEngine;

public class FlockingAttackDecisionTree
{
    private static bool anyBlueMiceSeesPlayer = false;
    private static Vector3? lastKnownPlayerPosition = null;
    private static bool flockIsRotating = false;
    private static float flockSearchTime = 5f;
    private static float rotationDuration = 5f;
    private static BlueMouse leaderMouse = null;

    private readonly EnemyController enemy;
    private readonly FSM fsm;
    private readonly FlockingAttackState attackState;
    private IDecisionNode rootNode;
    private MouseBoid mouseBoid;

    private float decisionInterval = 2f;
    private float decisionTimer = 0f;
    private float minAttackDistance = 0.01f;
    private float attackSpeed;

    public FlockingAttackDecisionTree(EnemyController enemy, FSM fsm, FlockingAttackState attackState)
    {
        this.enemy = enemy;
        this.fsm = fsm;
        this.attackState = attackState;
        this.attackSpeed = enemy.runSpeed;

        mouseBoid = enemy.GetComponent<MouseBoid>();
    }

    public void StartAttack()
    {
        CreateTree();
        decisionTimer = 0f;
    }

    public void Execute()
    {
        BlueMouse blueMouse = enemy as BlueMouse;
        bool thisMouseSeesPlayer = enemy.enemyVision.HasDirectDetection || enemy.enemyVision.HasPeripheralDetection;

        if (thisMouseSeesPlayer)
        {
            anyBlueMiceSeesPlayer = true;
            lastKnownPlayerPosition = enemy.enemyVision.LastSeenPosition;

            if (flockIsRotating)
            {
                BroadcastRotation(false, enemy.enemyVision.LastSeenPosition);
            }

            if (blueMouse != null && enemy.PlayerTransform != null)
            {
                BlueMouse.BroadcastToAllMice(
                    StateEnum.FlockingAttackState,
                    blueMouse,
                    enemy.PlayerTransform.position
                );
            }
        }

        if (!thisMouseSeesPlayer && anyBlueMiceSeesPlayer)
        {
            bool anyOtherMouseSeesPlayer = false;
            foreach (var mouse in BlueMouse.GetAllBlueMice())
            {
                if (mouse != null && mouse != blueMouse &&
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
                    leaderMouse = blueMouse;
                }
                BroadcastRotation(true, lastKnownPlayerPosition);
            }
        }

        if (flockIsRotating)
        {
            if (leaderMouse == blueMouse)
            {
                if (flockSearchTime > 0f)
                {
                    flockSearchTime -= Time.deltaTime;

                    if (flockSearchTime <= 0f)
                    {
                        BroadcastRotation(false);
                        BroadcastPatrolTransition(blueMouse);
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

                    if (blueMouse != null && enemy.PlayerTransform != null)
                    {
                        BlueMouse.BroadcastToAllMice(
                            StateEnum.FlockingAttackState,
                            blueMouse,
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
                leaderMouse = blueMouse;
            }
            BroadcastRotation(true, lastKnownPlayerPosition);
        }
    }

    private void BroadcastPatrolTransition(BlueMouse initiator)
    {
        fsm.Transition(StateEnum.FlockingPatrolState);
        
        foreach (var mouse in BlueMouse.GetAllBlueMice())
        {
            if (mouse != null && mouse != initiator)
            {
                mouse.TransitionToState(StateEnum.FlockingPatrolState);
            }
        }
    }

    private void BroadcastRotation(bool shouldRotate, Vector3? position = null)
    {
        if (flockIsRotating != shouldRotate)
        {
            flockIsRotating = shouldRotate;

            if (shouldRotate)
            {
                flockSearchTime = rotationDuration;
            }

            if (position.HasValue)
            {
                lastKnownPlayerPosition = position;
            }

            BlueMouse blueMouse = enemy as BlueMouse;
            if (blueMouse != null)
            {
                foreach (var mouse in BlueMouse.GetAllBlueMice())
                {
                    if (mouse != null && mouse != blueMouse)
                    {
                        State currentState = mouse.StateMachine.GetCurrentState();
                        FlockingAttackState attackState = currentState as FlockingAttackState;

                        if (attackState != null && attackState.GetDecisionTree() != null)
                        {
                            attackState.GetDecisionTree().SyncRotationState(shouldRotate, position);
                        }
                    }
                }
            }
        }
    }

    public void SyncRotationState(bool shouldRotate, Vector3? position)
    {
        flockIsRotating = shouldRotate;

        if (position.HasValue)
        {
            lastKnownPlayerPosition = position;
        }
    }

    private void MoveToPositionWithFlocking(Vector3 targetPosition)
    {
        if (mouseBoid == null) return;

        Vector3 dirToTarget = (targetPosition - enemy.transform.position).normalized;
        Vector3 seekForce = dirToTarget * attackSpeed;
        Vector3 avoidForce = enemy.obstacleAvoidance.Avoid() * 1.5f;
        Vector3 combinedForce = seekForce + avoidForce;

        mouseBoid.SetPathFollowingForce(combinedForce);
        mouseBoid.ApplyFlocking();
        mouseBoid.UpdateMovement();
    }

    private void CreateTree()
    {
        if (fsm == null || enemy == null)
        {
            return;
        }

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
        rootNode = new QuestionNode(atacar, estaGirando, () => anyBlueMiceSeesPlayer || enemy.enemyVision.HasDirectDetection || enemy.enemyVision.HasPeripheralDetection);
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