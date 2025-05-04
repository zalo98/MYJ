using UnityEngine;

public class EnemyInteractable : MonoBehaviour, IInteractable
{
    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError("EnemyInteractable requires an EnemyController component");
        }
    }

    public void Interact()
    {
        CollectibleEnemy collectible = enemyController.GetComponent<CollectibleEnemy>();

        if (collectible != null)
        {
            collectible.OnCollected();
            Destroy(enemyController.gameObject);
        }
    }
} 