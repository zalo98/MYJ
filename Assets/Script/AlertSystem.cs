using System.Collections.Generic;
using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    private static AlertSystem instance;
    public static AlertSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AlertSystem");
                instance = go.AddComponent<AlertSystem>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private List<EnemyController> enemiesInPatrol = new List<EnemyController>();
    private List<EnemyController> enemiesInLooking = new List<EnemyController>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        if (!enemiesInPatrol.Contains(enemy))
        {
            enemiesInPatrol.Add(enemy);
        }
    }

    public void RegisterEnemyInLooking(EnemyController enemy)
    {
        if (!enemiesInLooking.Contains(enemy))
        {
            enemiesInLooking.Add(enemy);
        }
    }

    public void UnregisterEnemy(EnemyController enemy)
    {
        enemiesInPatrol.Remove(enemy);
        enemiesInLooking.Remove(enemy);
    }

    public void AlertEnemies(Vector3 position)
    {
        List<EnemyController> enemiesToAlert = new List<EnemyController>(enemiesInPatrol);
        
        foreach (var enemy in enemiesToAlert)
        {
            if (enemiesInPatrol.Contains(enemy) && enemy.StateMachine._currentState == enemy.PatrolState)
            {
                enemy.StateMachine.Transition(StateEnum.EnemyAlert);
                if (enemy.StateMachine._currentState is AlertState alertState)
                {
                    alertState.decisionTree.AlertByCamera(position);
                }
            }
        }
        
        List<EnemyController> lookingEnemiesToAlert = new List<EnemyController>(enemiesInLooking);
        
        foreach (var enemy in lookingEnemiesToAlert)
        {
            if (enemiesInLooking.Contains(enemy) && enemy.StateMachine._currentState == enemy.EnemylookingState)
            {
                enemy.StateMachine.Transition(StateEnum.EnemyAlert);
                if (enemy.StateMachine._currentState is AlertState alertState)
                {
                    alertState.decisionTree.AlertByCamera(position);
                }
            }
        }
    }
} 