using System.Collections;
using UnityEngine;

public class EnemyLookingState : State
{
    private EnemyController controller;
    private Coroutine lookingCoroutine;
    private MonoBehaviour coroutineHost;

    public EnemyLookingState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
        this.coroutineHost = controller;
    }

    public override void Awake()
    {
        lookingCoroutine = coroutineHost.StartCoroutine(LookAroundCoroutine());
    }

    public override void Execute()
    {
        if (controller.enemyVision.HasDirectDetection)
        {
            controller.StateMachine.Transition(StateEnum.Attack);
        }
    }

    public override void Sleep()
    {
        if (lookingCoroutine != null)
        {
            coroutineHost.StopCoroutine(lookingCoroutine);
            lookingCoroutine = null;
        }
    }

    private IEnumerator LookAroundCoroutine()
    {
        float startTime = Time.time;
        
        Quaternion leftRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y - 45, 0);
        while (Time.time < startTime + 1f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, leftRotation, (Time.time - startTime));
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        startTime = Time.time;
        Quaternion rightRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y + 90, 0);
        while (Time.time < startTime + 1.5f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, rightRotation, (Time.time - startTime) / 1.5f);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        startTime = Time.time;
        Quaternion centerRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y - 45, 0);
        while (Time.time < startTime + 1f)
        {
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, centerRotation, (Time.time - startTime));
            yield return null;
        }
        
        controller.WaypointSystem.MoveToNextTarget();
        controller.StateMachine.Transition(StateEnum.EnemyPatrol);
    }
}