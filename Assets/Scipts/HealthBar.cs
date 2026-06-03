using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    private Slider healthSlider;
    private HealthLogic healthLogic;

    private Transform mainCameraTransform;

    void Start()
    {
        healthSlider = GetComponentInChildren<Slider>();
        healthLogic = GetComponentInParent<HealthLogic>();
        mainCameraTransform = Camera.main.transform;

        if (healthLogic != null)
        {
            healthLogic.onHealthChanged.AddListener(UpdateHealthBar);

            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth) => healthSlider.value = currentHealth / maxHealth;

}