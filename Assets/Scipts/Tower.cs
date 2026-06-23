using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : MonoBehaviour, IInteractable, IBuildable
{
    protected GameManager _hub;
    protected InteractableOutline _outlineController;
    [Header("Construction Settings")]
    [SerializeField] protected string _towerName;
    [field: SerializeField] public string Description { get; set; }
    [SerializeField] protected SphereCollider _rangeCollider;
    [field: SerializeField] public GameObject ConstructonBase { get; set; }
    public virtual string Name { get => _towerName; set => _towerName = value; }
    public float ConstructionRadius { get; set; }
    public Collider BaseCollider { get; set; }

    #region Stats
    [Header("Stats Configuration")]
    [SerializeField] protected DamageType _damageType = DamageType.Physical;
    public DamageType CurrentDamageType => _damageType;
    [SerializeField] protected AttackType _attackType = AttackType.Projectile;
    public AttackType CurrentAttackType => _attackType;
    [SerializeField] protected TargetPriority _targetPriority = TargetPriority.First;
    public TargetPriority CurrentTargetPriority => _targetPriority;
    [SerializeField] protected int _targetsAmount = 1;
    [SerializeField] protected TowerData _stats;
    protected bool IsOperational { get; private set; }
    public float Damage { get; private set; }
    public float Range { get; private set; }
    public float FireRate { get; private set; }

    [HideInInspector] public float DamageMultiplier = 1f;
    [HideInInspector] public float DamageFlatBonus = 0f;

    [HideInInspector] public float RangeMultiplier = 1f;
    [HideInInspector] public float RangeFlatBonus = 0f;

    [HideInInspector] public float FireRateMultiplier = 1f;
    [HideInInspector] public float FireRateFlatBonus = 0f;

    public event Action OnTowerStatsRecalculated;

    #endregion

    protected float _attackCountdown = 0f;

    protected List<HealthLogic> _enemiesInRange = new List<HealthLogic>();

    public delegate void TowerAttackHandler(AttackType attackType, Projectile projectile, ref ProjectileData data);
    public event TowerAttackHandler OnAttackExecuted;
    protected void NotifyAttackExecuted(AttackType attackType, Projectile projectile, ref ProjectileData data)
    {
        OnAttackExecuted?.Invoke(attackType, projectile, ref data);
    }

    protected void Awake()
    {
        _hub = GameManager.Instance;
        _outlineController = GetComponent<InteractableOutline>();
        if (DamageMultiplier == 0f) DamageMultiplier = 1f;
        if (RangeMultiplier == 0f) RangeMultiplier = 1f;
        if (FireRateMultiplier == 0f) FireRateMultiplier = 1f;
        BaseCollider = ConstructonBase.GetComponent<Collider>();
    }
    public void Initialize(string name)
    {
        RecalculateStats();
        enabled = true;
        IsOperational = true;
        OnPowerOn();
        Name = name;
        _outlineController = GetComponent<InteractableOutline>();
        _rangeCollider.gameObject.layer = _hub.Layers.TowerRangeIndicator;
        GetCountdown();
        UpdateRangeCollider();
        OnTowerStatsRecalculated += UpdateRangeCollider;
    }

    public void RecalculateStats()
    {
        Damage = (_stats.BaseDamage * DamageMultiplier) + DamageFlatBonus;
        Range = (_stats.BaseRange * RangeMultiplier) + RangeFlatBonus;
        FireRate = (_stats.BaseFireRate * FireRateMultiplier) + FireRateFlatBonus;

        Damage = Mathf.Max(0f, Damage);
        Range = Mathf.Max(0f, Range);
        FireRate = Mathf.Max(0f, FireRate);

        OnTowerStatsRecalculated?.Invoke();
    }
    private void UpdateRangeCollider()
    {
        _rangeCollider.isTrigger = true;
        _rangeCollider.radius = Range;
        Vector3 worldTargetCenter = new Vector3(transform.position.x, ((IBuildable)this).Bottom.y, transform.position.z);
        _rangeCollider.center = transform.InverseTransformPoint(worldTargetCenter);
    }

    public void HandleEnemyEnter(Collider other)
    {
        if (other.TryGetComponent<HealthLogic>(out var enemyHealth))
        {
            if (!_enemiesInRange.Contains(enemyHealth))
            {
                _enemiesInRange.Add(enemyHealth);
            }
        }
    }

    public void HandleEnemyExit(Collider other)
    {
        if (other.TryGetComponent<HealthLogic>(out var enemyHealth))
        {
            if (_enemiesInRange.Contains(enemyHealth))
            {
                _enemiesInRange.Remove(enemyHealth);
            }
        }
    }

    protected void UpdateTarget()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null);

        if (_enemiesInRange.Count == 0)
        {
            ResetTarget();
            return;
        }

        switch (_targetPriority)
        {
            case TargetPriority.First:
                break;

            case TargetPriority.Last:
                _enemiesInRange.Reverse();
                break;

            case TargetPriority.LowestCurrentHealth:
                _enemiesInRange.Sort((a, b) => a.GetCurrentHealth().CompareTo(b.GetCurrentHealth()));
                break;

            case TargetPriority.HighestMaxHealth:
                _enemiesInRange.Sort((a, b) => b.GetMaxHealth().CompareTo(a.GetMaxHealth()));
                break;

            case TargetPriority.Nearest:
                _enemiesInRange.Sort((a, b) =>
                    Vector3.Distance(transform.position, a.transform.position)
                    .CompareTo(Vector3.Distance(transform.position, b.transform.position)));
                break;

            case TargetPriority.Farthest:
                _enemiesInRange.Sort((a, b) =>
                    Vector3.Distance(transform.position, b.transform.position)
                    .CompareTo(Vector3.Distance(transform.position, a.transform.position)));
                break;

            case TargetPriority.Random:
                for (int i = 0; i < _enemiesInRange.Count; i++)
                {
                    var temp = _enemiesInRange[i];
                    int randomIndex = UnityEngine.Random.Range(i, _enemiesInRange.Count);
                    _enemiesInRange[i] = _enemiesInRange[randomIndex];
                    _enemiesInRange[randomIndex] = temp;
                }
                break;
        }
    }

    protected void GetCountdown()
    {
        if(FireRate == 0f) IsOperational = false;
        else _attackCountdown = 1f / FireRate;
    }

    protected virtual void Update()
    {
        if (!IsOperational) return;

        if (_attackCountdown > 0f)
        {
            _attackCountdown -= Time.deltaTime;
        }
    }
    public bool SwitchPowerState()
    {
        IsOperational = !IsOperational;
        if (IsOperational) OnPowerOn();
        else OnPowerOff();
        return IsOperational;
    }

    public bool CanInteract() => enabled;
    public string GetInteractPrompt() => $"Press {_hub.GetInteractKeyName()} to configure the tower ({Name})";
    public void Interact() => _hub.Menus.TowerMenu.OpenMenu(this);
    public void DisplayOutline(bool show) => _outlineController.ToggleOutline(show);
    protected virtual void OnPowerOn()
    {
        _rangeCollider.enabled = true;
        InvokeRepeating(nameof(UpdateTarget), 0f, 2f);
        GetCountdown();
    }
    protected virtual void OnPowerOff()
    {
        _rangeCollider.enabled = false;
        CancelInvoke(nameof(UpdateTarget));
    }

    protected virtual void Attack()
    {
        GetCountdown();
    }
    protected virtual void ResetTarget() { }
}