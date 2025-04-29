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

    public void UnregisterEnemy(EnemyController enemy)
    {
        enemiesInPatrol.Remove(enemy);
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
    }
} 