using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class EnemyStateMachine
{
    private EnemyController controller;
    private IEnemyState currentState;

    public void Initialize(EnemyController enemyController)
    {
        controller = enemyController;
        // Iniciar en estado de patrulla
        ChangeState(controller.PatrolState);
    }

    public void UpdateState()
    {
        if (currentState != null)
            currentState.UpdateState(controller);
    }

    public void ChangeState(IEnemyState newState)
    {
        // Salir del estado actual si existe
        if (currentState != null)
            currentState.ExitState(controller);

        // Cambiar al nuevo estado
        currentState = newState;

        // Entrar al nuevo estado
        if (currentState != null)
            currentState.EnterState(controller);
    }

    // Obtener el estado actual (útil para debugging)
    public IEnemyState GetCurrentState()
    {
        return currentState;
    }
}