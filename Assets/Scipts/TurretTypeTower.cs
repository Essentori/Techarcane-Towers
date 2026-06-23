using System.Collections;
using UnityEngine;

public class TurretTypeTower : Tower
{
    // TODO: Make it abstract so it can be inherited
    [Header("Turret Type Tower Configuration")]
    [SerializeField] private Transform _partToRotate;
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _rotationUpMaxAngle = 45f;
    [SerializeField] private float _rotationDownMaxAngle = -45f;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _attackConeAngle;
    [SerializeField] private float _projectileSpeed;

    protected override void Update()
    {
        if (!IsOperational) return;
        if (_enemiesInRange.Count > 0 && _enemiesInRange[0] == null)
        {
            UpdateTarget();
            return;
        }

        base.Update();

        if (_enemiesInRange.Count == 0 || _enemiesInRange[0] == null)
        {
            ResetTarget();
            return;
        }
        else if (_returnRotationCoroutine != null)
        {
            StopCoroutine(_returnRotationCoroutine);
            _returnRotationCoroutine = null;
        }

        RotateTowardsTarget(_enemiesInRange[0].transform.position);

        if (_attackCountdown <= 0f)
        {
            if (IsAimingAtTarget(_enemiesInRange[0].transform.position))
            {
                Attack();
            }
        }
    }

    private void RotateTowardsTarget(Vector3 enemyPosition)
    {
        Vector3 direction = enemyPosition - _partToRotate.position;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        _partToRotate.rotation = Quaternion.RotateTowards(_partToRotate.rotation, lookRotation, _turnSpeed * Time.deltaTime * 10f);

        Vector3 localEuler = _partToRotate.localEulerAngles;
        float pitch = localEuler.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -_rotationUpMaxAngle, -_rotationDownMaxAngle);
        _partToRotate.localEulerAngles = new Vector3(pitch, localEuler.y, 0f);
    }

    private bool IsAimingAtTarget(Vector3 enemyPosition)
    {
        Vector3 directionToTarget = (enemyPosition - _firePoint.position).normalized;
        float angle = Vector3.Angle(_firePoint.forward, directionToTarget);
        return angle <= _attackConeAngle;
    }

    protected override void Attack()
    {
        if (!IsOperational || _enemiesInRange.Count == 0) return;

        int targetsToShoot = Mathf.Min(_targetsAmount, _enemiesInRange.Count);

        for (int i = 0; i < targetsToShoot; i++)
        {
            HealthLogic currentEnemy = _enemiesInRange[i];
            if (currentEnemy == null) continue;

            GameObject projectileObject = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);

            Vector3 directionToTarget = (currentEnemy.transform.position - _firePoint.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                projectileObject.transform.rotation = Quaternion.LookRotation(directionToTarget);
            }

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                ProjectileData projectileData = new()
                {
                    Target = currentEnemy.transform,
                    ModifiedDamage = Damage,
                    Speed = _projectileSpeed,
                    DamageType = _damageType,
                    BaseDamage = _stats.BaseDamage
                };

                NotifyAttackExecuted(_attackType, projectile, ref projectileData);
                projectile.Initialize(projectileData); 
            }
        }

        base.Attack();
    }

    private Coroutine _returnRotationCoroutine;
    private IEnumerator ReturnToDefaultRotationRoutine()
    {
        while (Quaternion.Angle(_partToRotate.localRotation, Quaternion.identity) > 0.1f)
        {
            _partToRotate.localRotation = Quaternion.RotateTowards(
                _partToRotate.localRotation,
                Quaternion.identity,
                _turnSpeed * Time.deltaTime * 10f
            );
            yield return null;
        }
        _partToRotate.localRotation = Quaternion.identity;
    }

    protected override void OnPowerOn()
    {
        base.OnPowerOn();
        if (_returnRotationCoroutine != null)
        {
            StopCoroutine(_returnRotationCoroutine);
            _returnRotationCoroutine = null;
        }
    }

    protected override void OnPowerOff()
    {
        base.OnPowerOff();
        if (_returnRotationCoroutine == null)
        {
            _returnRotationCoroutine = StartCoroutine(ReturnToDefaultRotationRoutine());
        }
    }
    protected override void ResetTarget()
    {
        if (_returnRotationCoroutine == null)
        {
            _returnRotationCoroutine = StartCoroutine(ReturnToDefaultRotationRoutine());
        }
    }
}