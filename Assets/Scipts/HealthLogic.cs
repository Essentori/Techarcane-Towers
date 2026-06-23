using UnityEngine;
using UnityEngine.Events;

public class HealthLogic : MonoBehaviour
{
    // TODO: Rename, Armor, Shields (different types), Effects (buffs, debuffs, DoTs, and etc)
    private float _maxHealth = 100f;
    public float GetMaxHealth() => _maxHealth;
    private float _currentHealth;
    public float GetCurrentHealth() => _currentHealth;

    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent OnDeath;

    void Start()
    {
        _currentHealth = _maxHealth;
    }
    public void TakeDamage(float damageAmount)
    {
        if (_currentHealth <= 0f || damageAmount == 0) return;
        if (damageAmount < 0f)
        {
            Heal(-damageAmount);
            return;
        }
        UpdateHealth(-damageAmount);
    }
    public void Heal(float healAmount)
    {
        if (_currentHealth <= 0f || healAmount == 0) return;
        if (healAmount < 0f)
        {
            TakeDamage(-healAmount);
            return;
        }
        UpdateHealth(healAmount);
    }

    private void UpdateHealth(float changeAmount)
    {
        _currentHealth += changeAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0f) Kill();
    }

    private void Kill()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}