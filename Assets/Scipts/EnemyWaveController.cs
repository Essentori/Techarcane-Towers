using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyWaveController : MonoBehaviour
{
    // TODO: Make it as GLOBAL game state
    private enum WaveState { WaitingNextWave, Spawning, WaitingForClear }

    [Header("References")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _defenceTarget;
    [SerializeField] private GameObject _modulePickupPrefab;
    [SerializeField] private TextMeshProUGUI _waveTimerText;
    [SerializeField] private TextMeshProUGUI _enemyCounterText;

    [Header("Wave Settings")]
    public float TimeBetweenWaves = 10f;
    public float EnemySpawnInterval = 1.5f;
    public int FirstWaveEnemyCount = 3;

    [Header("Drop Rate Settings")]
    [Range(0f, 1f)] public float GuaranteeDropPercent = 0.1f;
    [Range(0f, 1f)] public float BonusDropChance = 0.04f;

    private int _currentWave = 0;
    private int _enemiesToSpawnThisWave;
    private int _enemiesRemainingAlive;
    private WaveState _currentState = WaveState.WaitingNextWave;
    private float _stateTimer;

    private List<bool> _waveDropPlan = new List<bool>();
    private int _spawnedEnemiesCounter = 0;
    private Vector3 _defencePosition;

    void Start()
    {
        _defencePosition = _defenceTarget.transform.position;
        PrepareNextWave();
    }

    void Update()
    {
        switch (_currentState)
        {
            case WaveState.WaitingNextWave:
                RunBreakState();
                break;
            case WaveState.WaitingForClear:
                RunWaitingState();
                break;
        }

        UpdateUI();
    }

    private void PrepareNextWave()
    {
        _currentWave++;
        _enemiesToSpawnThisWave = FirstWaveEnemyCount + (_currentWave - 1);
        _enemiesRemainingAlive = _enemiesToSpawnThisWave;
        _spawnedEnemiesCounter = 0;

        GenerateWaveDropPlan();

        _stateTimer = TimeBetweenWaves;
        _currentState = WaveState.WaitingNextWave;
    }

    private void GenerateWaveDropPlan()
    {
        _waveDropPlan.Clear();
        int guaranteedDropsCount = Mathf.Max(1, Mathf.FloorToInt(_enemiesToSpawnThisWave * GuaranteeDropPercent));

        for (int i = 0; i < _enemiesToSpawnThisWave; i++)
        {
            _waveDropPlan.Add(i < guaranteedDropsCount);
        }
        for (int i = 0; i < _waveDropPlan.Count; i++)
        {
            bool temp = _waveDropPlan[i];
            int randomIndex = Random.Range(i, _waveDropPlan.Count);
            _waveDropPlan[i] = _waveDropPlan[randomIndex];
            _waveDropPlan[randomIndex] = temp;
        }
    }

    private void RunBreakState()
    {
        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0f)
        {
            StartCoroutine(SpawnWaveRoutine());
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        _currentState = WaveState.Spawning;

        for (int i = 0; i < _enemiesToSpawnThisWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(EnemySpawnInterval);
        }

        _currentState = WaveState.WaitingForClear;
    }

    private void SpawnEnemy()
    {
        Vector3 randomSpawnOffset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        Vector3 spawnPosition = transform.position + randomSpawnOffset;

        GameObject enemyObj = Instantiate(_enemyPrefab, spawnPosition, transform.rotation);

        if (enemyObj.TryGetComponent<EnemyAI>(out var enemyAI))
        {
            enemyAI.Agent.SetDestination(_defencePosition);
        }

        if (enemyObj.TryGetComponent<HealthLogic>(out var health))
        {
            bool isGuaranteed = _waveDropPlan[_spawnedEnemiesCounter];
            bool isBonus = !isGuaranteed && (Random.value < BonusDropChance);
            bool carriesModule = isGuaranteed || isBonus;
            string dropType = isGuaranteed ? "guarantee" : "bonus";

            if (carriesModule && enemyAI != null)
            {
                enemyAI.SetDropGlow(true);
            }

            health.OnDeath.AddListener(() =>
            {
                if (carriesModule)
                {
                    Debug.Log($"<color=green>[DROP]</color> Enemy dropped {dropType} module!");
                    GameObject module = Instantiate(_modulePickupPrefab, enemyObj.transform.position, Quaternion.identity);
                    module.GetComponent<ModulePickup>().Initialize(null);
                }
                _enemiesRemainingAlive--;
            });
        }
        _spawnedEnemiesCounter++;
    }

    private void RunWaitingState()
    {
        if (_enemiesRemainingAlive <= 0)
        {
            PrepareNextWave();
        }
    }

    private void UpdateUI()
    {
        if (_waveTimerText != null)
        {
            if (_currentState == WaveState.WaitingNextWave)
                _waveTimerText.text = $"Next wave {_currentWave}: {Mathf.CeilToInt(_stateTimer)} sec";
            else
                _waveTimerText.text = $"Current wave: {_currentWave}";
        }

        if (_enemyCounterText != null)
        {
            _enemyCounterText.text = $"Enemies left: {Mathf.Max(0, _enemiesRemainingAlive)}";
        }
    }
}