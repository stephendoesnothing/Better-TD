using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private int cashReward;

    private float currentHealth;

    public event Action OnDeath;
    public event Action OnHealthChanged; // New event for health bar updates

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(); // Notify health bar of change

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.EnemyKilled(cashReward);
        }

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public void ReachedEndWithHealth()
    {
        int livesToRemove = Mathf.CeilToInt(currentHealth);
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RemoveLives(livesToRemove);
        }

        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}