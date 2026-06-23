using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Config/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Stats")]
    [Tooltip("BASE STATS, DO NOT MODIFY IN SCRIPT")]
    public float BaseDamage;
    public float BaseRange;
    public float BaseFireRate;
}