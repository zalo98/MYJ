using System.Collections;
using UnityEngine;

public class EnemyLookingState : State
{
    private EnemyController controller;
    private Coroutine lookingCoroutine;
    private MonoBehaviour coroutineHost;
    private float lookDuration = 5f;

    public EnemyLookingState(EnemyController controller, FSM fsm) : base(fsm)
    {
        this.controller = controller;
        this.coroutineHost = controller;
    }

    public override void Awake()
    {
        controller.EnemyAnimator.SetBool("IsWalking", false);
        controller.EnemyAnimator.SetBool("IsRunning", false);
        controller.EnemyAnimator.SetBool("IsLooking", true);

        AlertSystem.Instance.RegisterEnemyInLooking(controller);

        lookingCoroutine = coroutineHost.StartCoroutine(LookAroundCoroutine());
    }

    public override void Execute()
    {
        if (controller.enemyVision.HasDirectDetection)
        {
            controller.StateMachine.Transition(StateEnum.Attack);
        }
        else if (controller.enemyVision.HasPeripheralDetection)
        {
            controller.StateMachine.Transition(StateEnum.EnemyAlert);
        }
    }

    public override void Sleep()
    {
        if (lookingCoroutine != null)
        {
            coroutineHost.StopCoroutine(lookingCoroutine);
            lookingCoroutine = null;
        }

        controller.EnemyAnimator.SetBool("IsLooking", false);
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
        
        controller.mouseMovement.MoveToNextTarget();
        controller.StateMachine.Transition(StateEnum.EnemyPatrol);
    }
}