using UnityEngine;

public class EnemyHoverDetector : MonoBehaviour
{
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnMouseEnter()
    {
        if (enemyHealth != null && EnemyHealthBarUI.Instance != null)
        {
            EnemyHealthBarUI.Instance.ShowHealthBar(enemyHealth);
        }
    }

    private void OnMouseExit()
    {
        if (EnemyHealthBarUI.Instance != null)
        {
            EnemyHealthBarUI.Instance.HideHealthBar();
        }
    }

    private void OnDestroy()
    {
        // Hide health bar if this enemy is destroyed while being hovered
        if (EnemyHealthBarUI.Instance != null &&
            EnemyHealthBarUI.Instance.GetTrackedEnemy() == enemyHealth)
        {
            EnemyHealthBarUI.Instance.HideHealthBar();
        }
    }
}
