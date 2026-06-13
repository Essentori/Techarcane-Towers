using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;

    public float speed = 20f;
    public GameObject hitEffectPrefab;

    public void Initialize(Transform _target, float _damage)
    {
        target = _target;
        damage = _damage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    protected virtual void HitTarget()
    {
        HealthLogic enemyHealth = target.GetComponent<HealthLogic>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
        if (hitEffectPrefab != null)
        {
            Destroy(Instantiate(hitEffectPrefab, transform.position, transform.rotation), 1f);
            return;
        }
        Destroy(gameObject);
    }
}