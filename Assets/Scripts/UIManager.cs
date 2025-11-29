using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI cashText;
    [SerializeField] private TextMeshProUGUI livesText;

    private GameManager gameManager;
    private EnemySpawner enemySpawner;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) Debug.LogError("Game Manager not found :(");

        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner == null) Debug.LogError("Enemy Spawner not found :(");
    }

    private void OnDestroy()
    {
    }

    private void Update()
    {
        if (gameManager == null) return;

        // Update cash and lives display
        if (cashText != null)
        {
            cashText.text = "Cash : $" + gameManager.CurrentCash.ToString();
        }

        if (livesText != null)
        {
            livesText.text = "Lives : " + gameManager.CurrentLives.ToString();
        }
    }

    private void OnCurrencyAwarded(int amount)
    {
        if (gameManager != null)
        {
            gameManager.AddCash(amount);
            Debug.Log($"Skip wave reward: +${amount}");
        }
    }
}