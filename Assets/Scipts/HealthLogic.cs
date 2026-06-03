using UnityEngine;
using UnityEngine.Events;
public class HealthLogic : MonoBehaviour
{
    private float maxHealth = 100f;
    private float currentHealth;

    [HideInInspector] public UnityEvent<float, float> onHealthChanged;
    [HideInInspector] public UnityEvent onDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}