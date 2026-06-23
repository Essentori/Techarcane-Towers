using UnityEngine;

public interface IBuildable
{
    public string Name { get; set; }
    public string Description { get; set; }
    GameObject ConstructonBase { get; set; }
    float ConstructionRadius { get; set; }
    Collider BaseCollider { get; set; }
    public Vector3 Bottom => BaseCollider.bounds.min;
    public void Initialize(string Name);
    public void CalculateRadius()
    {
        Vector3 extents = BaseCollider.bounds.extents;
        ConstructionRadius = Mathf.Max(extents.x, extents.z);
    }
}