using UnityEngine;

public class TWR_LightTurret : Tower
{

    [Header("Settings")]
    [SerializeField] private float _fireRate = 4f;
    [SerializeField] private float _damage = 5f;
    private float Range = 20f;

    [Header("Rotating Part Configuration")]
    [SerializeField] private Transform _partToRotate;
    [SerializeField] private float _turnSpeed = 10f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _attackConeAngle = 10f;
    [SerializeField] private bool _isAoE = false;

    private float _fireCountdown = 0f;
    private Transform _target;
    private HealthLogic _targetHealth;

    protected override void ActivateTower()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
    }

    private void Update()
    {
        if (!IsOperational)
        {
            ReturnToDefaultRotation();
            return;
        }

        if (_target != null)
        {
            RotateTowardsTarget();

            if (_fireCountdown <= 0f)
            {
                if (_isAoE || IsAimingAtTarget())
                {
                    Shoot();
                    _fireCountdown = 1f / _fireRate;
                }
            }
        }
        else
        {
            ReturnToDefaultRotation();
        }

        if (_fireCountdown > 0f)
        {
            _fireCountdown -= Time.deltaTime;
        }
    }

    private bool IsAimingAtTarget()
    {
        if (_target == null || _firePoint == null) return false;
        Vector3 directionToTarget = (_target.position - _firePoint.position).normalized;
        float angle = Vector3.Angle(_firePoint.forward, directionToTarget);
        return angle <= _attackConeAngle;
    }

    private void UpdateTarget()
    {
        if (!IsOperational)
        {
            ResetTarget();
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, Range, _hub.Layers.Enemy);
        if (colliders.Length == 0)
        {
            ResetTarget();
            return;
        }

        Collider chosenEnemy = null;
        float maxDistance = -Mathf.Infinity;
        float minDistance = Mathf.Infinity;
        float minHealth = Mathf.Infinity;
        float maxHealth = -Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            HealthLogic health = col.GetComponent<HealthLogic>();
            if (health == null) continue;

            float currentDistance = Vector3.Distance(col.transform.position, transform.position);
            float currentHealth = health.GetCurrentHealth();
            switch (_targetPriority)
            {
                case TargetPriority.First:
                    if (currentDistance > maxDistance)
                    {
                        maxDistance = currentDistance;
                        chosenEnemy = col;
                    }
                    break;

                case TargetPriority.Last:
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        chosenEnemy = col;
                    }
                    break;

                case TargetPriority.LowestHealth:
                    if (currentHealth < minHealth)
                    {
                        minHealth = currentHealth;
                        chosenEnemy = col;
                    }
                    break;

                case TargetPriority.HighestHealth:
                    if (currentHealth > maxHealth)
                    {
                        maxHealth = currentHealth;
                        chosenEnemy = col;
                    }
                    break;
            }
        }

        if (chosenEnemy != null)
        {
            _target = chosenEnemy.transform;
            _targetHealth = chosenEnemy.GetComponent<HealthLogic>();
        }
        else
        {
            ResetTarget();
        }
    }

    private void RotateTowardsTarget()
    {
        if (_target == null || _partToRotate == null) return;

        Vector3 direction = _target.position - _partToRotate.position;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        _partToRotate.rotation = Quaternion.RotateTowards(_partToRotate.rotation, lookRotation, _turnSpeed * Time.deltaTime * 10f);

        Vector3 localEuler = _partToRotate.localEulerAngles;
        float pitch = localEuler.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -45f, 45f);
        _partToRotate.localEulerAngles = new Vector3(pitch, localEuler.y, 0f);
    }

    private void Shoot()
    {
        if (_projectilePrefab == null || _firePoint == null) return;

        GameObject projGO = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(_target, _damage);
        }
    }

    private void ReturnToDefaultRotation()
    {
        if (_partToRotate == null) return;
        _partToRotate.localRotation = Quaternion.RotateTowards(_partToRotate.localRotation, Quaternion.identity, _turnSpeed * Time.deltaTime * 10f);
    }

    private void ResetTarget()
    {
        _target = null;
        _targetHealth = null;
    }

    protected override void OnPowerOn()
    {
        CancelInvoke(nameof(UpdateTarget));
        ResetTarget();
        _fireCountdown = 0f;
        ReturnToDefaultRotation();
    }

    protected override void OnPowerOff() { }
}