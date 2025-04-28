using System.Collections;
using UnityEngine;

public class LookingState : State
{
    private EnemyController controller;
    private Coroutine lookingCoroutine;
    private MonoBehaviour coroutineHost; // Para poder iniciar coroutines

    public LookingState(MonoBehaviour host, FSM fsm) : base(fsm)
    {
        coroutineHost = host;
    }

    public override void Awake()
    {
        // Configurar la animación al entrar al estado
        controller.EnemyAnimator.SetBool("IsWalking", false);
        controller.EnemyAnimator.SetBool("IsRunning", false);
        controller.EnemyAnimator.SetBool("IsLooking", true);

        // Iniciar la rutina de mirar alrededor
        lookingCoroutine = coroutineHost.StartCoroutine(LookAroundCoroutine(controller));
    }

    public override void Execute()
    {
        // Comprobar si el jugador está visible mediante visión directa o periférica
        if (controller.enemyVision.HasDirectDetection || (controller.enemyVision.usePeripheralVision && controller.enemyVision.HasPeripheralDetection))
        {
            // Si se detecta al jugador, cambiar al estado de escapar
            controller.StateMachine.Transition(StateEnum.EnemyEscape);
            return;
        }
        // La rotación y animación se manejan en la corrutina
    }

    public override void Sleep()
    {
        // Detener la corrutina si está en progreso
        if (lookingCoroutine != null)
            coroutineHost.StopCoroutine(lookingCoroutine);
    }

    IEnumerator LookAroundCoroutine(EnemyController controller)
    {
        // Mirar a la izquierda
        float startTime = Time.time;
        Quaternion startRotation = controller.transform.rotation;
        Quaternion leftRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y - 45, 0);
        while (Time.time < startTime + 1f)
        {
            controller.transform.rotation = Quaternion.Slerp(startRotation, leftRotation, (Time.time - startTime));
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        // Mirar a la derecha
        startTime = Time.time;
        startRotation = controller.transform.rotation;
        Quaternion rightRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y + 90, 0);
        while (Time.time < startTime + 1.5f)
        {
            controller.transform.rotation = Quaternion.Slerp(startRotation, rightRotation, (Time.time - startTime) / 1.5f);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);

        // Volver al centro
        startTime = Time.time;
        startRotation = controller.transform.rotation;
        Quaternion centerRotation = Quaternion.Euler(0, controller.transform.eulerAngles.y - 45, 0);
        while (Time.time < startTime + 1f)
        {
            controller.transform.rotation = Quaternion.Slerp(startRotation, centerRotation, (Time.time - startTime));
            yield return null;
        }

        // Volver a patrullar
        controller.WaypointSystem.MoveToNextTarget();
        controller.StateMachine.Transition(StateEnum.EnemyPatrol);
    }
}