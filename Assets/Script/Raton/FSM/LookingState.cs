using System.Collections;
using UnityEngine;

public class LookingState : State
{
    private EnemyController controller;
    private Coroutine lookingCoroutine;
    private MonoBehaviour coroutineHost;
    private float lookDuration = 5f;

    public LookingState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
        coroutineHost = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", false);
        controller.EnemyAnimator.SetBool("IsRunning", false);
        controller.EnemyAnimator.SetBool("IsLooking", true);

        lookingCoroutine = coroutineHost.StartCoroutine(LookAroundCoroutine(controller));
    }

    public override void Execute()
    {
        if (controller.enemyVision.HasDirectDetection)
        {
            controller.StateMachine.Transition(StateEnum.MouseEscapeState);
            return;
        }
    }

    public override void Sleep()
    {
        if (lookingCoroutine != null)
            coroutineHost.StopCoroutine(lookingCoroutine);

        controller.EnemyAnimator.SetBool("IsLooking", false);
    }

    IEnumerator LookAroundCoroutine(EnemyController controller)
    {
        float startTime = Time.time;
        
        // Mirar hacia la izquierda
        Quaternion leftRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y - 45, 0);
        while (Time.time < startTime + 1f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, leftRotation, (Time.time - startTime));
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Mirar hacia la derecha
        startTime = Time.time;
        Quaternion rightRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y + 90, 0);
        while (Time.time < startTime + 1.5f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, rightRotation, (Time.time - startTime) / 1.5f);
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);

        // Mirar de vuelta al centro
        startTime = Time.time;
        Quaternion centerRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y - 45, 0);
        while (Time.time < startTime + 1f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, centerRotation, (Time.time - startTime));
            yield return null;
        }

        // Volver a patrullar
        controller.WaypointSystem.MoveToNextTarget();
        controller.StateMachine.Transition(StateEnum.MousePatrolState);
    }
}