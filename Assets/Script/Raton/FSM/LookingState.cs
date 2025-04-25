using UnityEngine;
using System.Collections;

public class LookingState : MonoBehaviour, IEnemyState
{
    private Coroutine lookingCoroutine;

    public void EnterState(EnemyController controller)
    {
        // Configurar animación
        controller.EnemyAnimator.SetBool("IsWalking", false);
        controller.EnemyAnimator.SetBool("IsRunning", false);
        controller.EnemyAnimator.SetBool("IsLooking", true);

        // Iniciar la rutina de mirar alrededor
        lookingCoroutine = StartCoroutine(LookAroundCoroutine(controller));
    }

    public void UpdateState(EnemyController controller)
    {
        // Comprobar si el jugador está visible
        if (controller.LineOfSight.CanSeePlayer())
        {
            controller.StateMachine.ChangeState(controller.EscapingState);
            return;
        }

        // La rotación y animación se manejan en la corrutina
    }

    public void ExitState(EnemyController controller)
    {
        // Detener la corrutina si está en progreso
        if (lookingCoroutine != null)
            StopCoroutine(lookingCoroutine);
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
        controller.Steering.MoveToNextTarget();
        controller.StateMachine.ChangeState(controller.PatrolState);
    }
}
