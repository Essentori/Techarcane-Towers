using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _defenceTarget;
    [SerializeField] private Vector3 _defencePosition;
    public float EnemySpawnInterval = 2f;

    private float _timer;
    void Start()
    {
        _timer = EnemySpawnInterval;
        _defencePosition = _defenceTarget.transform.position;
    }

    void Update()
    {
        if ((_timer += Time.deltaTime) >= EnemySpawnInterval)
        {
            SpawnEnemy();
            _timer = 0f;
        }
    }
    void SpawnEnemy()
    {
        Vector3 randomSpawnOffset = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f));
        Vector3 spawnPosition = transform.position + randomSpawnOffset;
        EnemyAI spawnedEnemy = Instantiate(_enemyPrefab, spawnPosition, transform.rotation).GetComponent<EnemyAI>();
        spawnedEnemy.Agent.SetDestination(_defencePosition);
    }
}