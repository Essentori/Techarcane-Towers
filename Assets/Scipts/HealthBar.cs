using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // TODO:
    // Add: Armor, Shields (different types), Effects (buffs and debuffs) visuals;
    // Improve: changes feedback, optimization, visuals
    // Change: Shows only on demand (player nearby, or looking on)

    private Slider _healthSlider;
    private HealthLogic _healthLogic;

    private Transform _mainCameraTransform;

    void Start()
    {
        _healthSlider = GetComponentInChildren<Slider>();
        _healthLogic = GetComponentInParent<HealthLogic>();
        _mainCameraTransform = Camera.main.transform;

        if (_healthLogic != null)
        {
            _healthLogic.OnHealthChanged.AddListener(UpdateHealthBar);

            _healthSlider.maxValue = 1f;
            _healthSlider.value = 1f;
        }
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    void LateUpdate() => transform.LookAt(transform.position + _mainCameraTransform.forward);

    private void UpdateHealthBar(float currentHealth, float maxHealth) => _healthSlider.value = currentHealth / maxHealth;

}