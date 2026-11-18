
using UnityEngine;
using UnityEngine.InputSystem;

public class HealthTest : MonoBehaviour
{
    public FighterHealth playerHealth;
    public FighterHealth botHealth;

    public float damageAmount = 10f;

    private void Start()
    {
        Debug.Log("HealthTest is active!");

        if (playerHealth == null)
            Debug.LogError("Player Health is not assigned!");

        if (botHealth == null)
            Debug.LogError("Bot Health is not assigned!");
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("P pressed - damaging Player.");

            if (playerHealth != null)
                playerHealth.TakeDamage(damageAmount);
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("B pressed - damaging Bot.");

            if (botHealth != null)
                botHealth.TakeDamage(damageAmount);
        }
    }
}
