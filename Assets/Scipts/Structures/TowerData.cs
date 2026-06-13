
using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Config/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Stats")]
    public string BaseName;
    public float BasDamage;
    public float BaseRange;
    public float BaseFireRate;
}