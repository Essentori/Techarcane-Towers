using UnityEditor;
using UnityEngine;

public class LayersHandler : MonoBehaviour
{
    [field: Layer, SerializeField] public int Tower { get; private set; }
    [field: Layer, SerializeField] public int TowerRangeIndicator { get; private set; }
    [field: Layer, SerializeField] public int Enemy { get; private set; }
    [field: SerializeField] public LayerMask Interactable { get; private set; }
    [field: SerializeField] public LayerMask Buildable { get; private set; }
    [field: SerializeField] public LayerMask ObstaclesForBuilding { get; private set; }
    [field: SerializeField] public LayerMask Walkable { get; private set; }
    [field: Layer, SerializeField] public int BlueprintConstruction { get; private set; }
    [field: SerializeField] public RenderingLayerMask OutlinedLayer { get; private set; }
    [field: SerializeField] public RenderingLayerMask IgnoreDecal { get; private set; }
    [field: SerializeField] public RenderingLayerMask DefaultRenderingLayer { get; private set; }
    [field: Layer, SerializeField] public int IgnoreRaycast { get; private set; }
    public LayerMask GetLayerByType(ConstructionType type)
    {
        switch (type)
        {
            case ConstructionType.Tower: return Tower;
            default: return default;
        }
    }
}

public class LayerAttribute : PropertyAttribute { }
[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer)
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        else 
            EditorGUI.LabelField(position, label.text, $"Used {property.type}, expected INT value!");

    }
}
