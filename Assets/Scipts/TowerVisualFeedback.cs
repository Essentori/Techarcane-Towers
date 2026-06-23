using System.Collections;
using UnityEngine;

public class TowerVisualFeedback : MonoBehaviour
{
    // TODO: Make it better
    private TowerModulesManager _modulesManager;

    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem _feedbackParticlePrefab;
    [SerializeField] private Transform _spawnPoint;

    [Header("Material Flash Settings")]
    [SerializeField] private MeshRenderer _towerRenderer;
    [SerializeField] private Color _flashColor = Color.cyan;
    [SerializeField] private float _flashDuration = 0.1f;

    private Color _originalColor;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        _modulesManager = GetComponent<TowerModulesManager>();

        if (_towerRenderer != null)
        {
            _originalColor = _towerRenderer.material.color;
        }
    }

    private void OnEnable()
    {
        if (_modulesManager != null)
        {
            _modulesManager.OnModuleActionExecuted += PlayFeedback;
        }
    }

    private void OnDisable()
    {
        if (_modulesManager != null)
        {
            _modulesManager.OnModuleActionExecuted -= PlayFeedback;
        }
    }

    private void PlayFeedback()
    {
        if (_feedbackParticlePrefab != null)
        {
            Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : transform.position;
            Quaternion spawnRot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

            ParticleSystem fx = Instantiate(_feedbackParticlePrefab, spawnPos, spawnRot);
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        if (_towerRenderer != null)
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        _towerRenderer.material.color = _flashColor;
        yield return new WaitForSeconds(_flashDuration);
        _towerRenderer.material.color = _originalColor;
    }
}