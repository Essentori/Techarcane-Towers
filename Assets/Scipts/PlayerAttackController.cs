using UnityEngine;
using UnityEngine.InputSystem; // Не забываем новый Input System

public class PlayerAttackController : MonoBehaviour
{
    [SerializeField] private float damage = 25f;

    void Update()
    {
        if (Pointer.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Pointer.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HealthLogic enemyHealth = hit.collider.GetComponent<HealthLogic>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }
    }
}