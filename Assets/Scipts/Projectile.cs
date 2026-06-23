using UnityEngine;
public struct ProjectileData
{
    public Transform Target;
    public float BaseDamage;
    public float ModifiedDamage;
    public DamageType DamageType;
    public float Speed;
    public GameObject HitEffect;
}
public class Projectile : MonoBehaviour
{
    private ProjectileData _data;
    [SerializeField] private GameObject HitEffectPrefab;
    private Vector3 _initialScale;
    public void Initialize(ProjectileData data)
    {
        _data = data;
        _data.HitEffect = HitEffectPrefab;
        _initialScale = transform.localScale;
        ApplyDynamicScaling();
    }
    private void ApplyDynamicScaling()
    {
        if (_data.BaseDamage <= 0f) return;
        float damageIncreasePct = (_data.ModifiedDamage - _data.BaseDamage) / _data.BaseDamage;
        // Ratio: increase rate of damage to size (damage : size) - 1:4
        float scaleIncreasePct = damageIncreasePct / 4f;
        float scaleMultiplier = Mathf.Max(0.1f, 1f + scaleIncreasePct);
        transform.localScale = _initialScale * scaleMultiplier;
    }

    void Update()
    {
        if (_data.Target == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 dir = _data.Target.position - transform.position;
        float distanceThisFrame = _data.Speed * Time.deltaTime;
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(_data.Target);
    }

    protected virtual void HitTarget()
    {
        HealthLogic enemyHealth = _data.Target.GetComponent<HealthLogic>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(_data.ModifiedDamage);
        }
        if (_data.HitEffect != null)
        {
            Destroy(Instantiate(HitEffectPrefab, transform.position, transform.rotation), 1f);
            return;
        }
        Destroy(gameObject);
    }
}