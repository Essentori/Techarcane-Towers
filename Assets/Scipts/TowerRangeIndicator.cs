using UnityEngine;

public class TowerRangeIndicator : MonoBehaviour
{
    [SerializeField] private Tower _parentTower;
    private void Start()
    {
        _parentTower = GetComponentInParent<Tower>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        _parentTower.HandleEnemyEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _parentTower.HandleEnemyExit(other);
    }
}