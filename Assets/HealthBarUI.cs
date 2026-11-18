
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public FighterHealth fighterHealth;
    public Slider healthSlider;

    private void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (fighterHealth != null)
        {
            fighterHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void Start()
    {
        if (fighterHealth == null || healthSlider == null)
        {
            Debug.LogError(
                "HealthBarUI: Missing references on " + gameObject.name
            );
            enabled = false;
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = fighterHealth.maxHealth;

        // Display the initial health.
        UpdateHealthBar(
            fighterHealth.CurrentHealth,
            fighterHealth.maxHealth
        );
    }

    private void OnDisable()
    {
        if (fighterHealth != null)
        {
            fighterHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
