using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;

    private float timer;
    void Start() => timer = spawnInterval;

    void Update()
    {
        if ((timer += Time.deltaTime) >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Vector3 randomSpawnOffset = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            );
            Vector3 spawnPosition = transform.position + randomSpawnOffset;
            Instantiate(enemyPrefab, spawnPosition, transform.rotation);
        }
    }
}