
using System;
using UnityEngine;

public class FighterHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;

    public float CurrentHealth { get; private set; }

    public bool IsDead { get; private set; }

    // Notifies the health bar whenever health changes.
    public event Action<float, float> OnHealthChanged;

    // Notifies other systems when the fighter is defeated.
    public event Action OnDeath;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        CurrentHealth = Mathf.Max(
            0f,
            CurrentHealth - damage
        );

        Debug.Log(
            gameObject.name + " Health: " +
            CurrentHealth + " / " + maxHealth
        );

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        CurrentHealth = Mathf.Min(
            maxHealth,
            CurrentHealth + amount
        );

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        Debug.Log(gameObject.name + " Health Reset!");
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        Debug.Log(gameObject.name + " has been defeated!");

        OnDeath?.Invoke();
    }
}
