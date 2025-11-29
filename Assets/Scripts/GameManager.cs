using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int startingLives = 200;
    [SerializeField] private int startingCash = 500;

    private int currentLives;
    private int currentCash;
    private bool gameOver = false;

    public int CurrentLives => currentLives;
    public int CurrentCash => currentCash;

    private void Start()
    {
        currentLives = startingLives;
        currentCash = startingCash;
    }

    public void RemoveLives(int amount)
    {
        if (gameOver) return;

        currentLives -= amount;
        if(currentLives <= 0)
        {
            currentLives = 0;
            GameOver();
        }
    }

    public void EnemyKilled(int cashReward)
    {
        currentCash += cashReward;
    }

    public void AddCash(int amount)
    {
        currentCash += amount;
    }

    public void SpendCash(int amount)
    {
        currentCash -= amount;
    }

    private void GameOver()
    {
        gameOver = true;
        Debug.Log("Game Over!");

        Invoke(nameof(RestartGame), 2f);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
