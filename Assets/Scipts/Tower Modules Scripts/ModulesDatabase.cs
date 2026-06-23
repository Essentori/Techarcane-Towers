using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleDatabase", menuName = "Modules/Module Database")]
public class ModulesDatabase : ScriptableObject
{
    [Header("Available Modules Arrays")]
    [SerializeField] private List<ModuleConditionTypeData> _conditions = new List<ModuleConditionTypeData>();
    [SerializeField] private List<ModuleActionTypeData> _actions = new List<ModuleActionTypeData>();
    public List<ModuleConditionTypeData> Conditions => _conditions;
    public List<ModuleActionTypeData> Actions => _actions;
    private int GetTierWeight(ModuleTier tier)
    {
        return tier switch
        {
            ModuleTier.Common => 75,
            ModuleTier.Upgraded => 25,
            ModuleTier.Arcane => 5,
            _ => 0
        };
    }
    public ModuleConditionTypeData RollRandomCondition()
    {
        if (_conditions == null || _conditions.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        foreach (var cond in _conditions)
        {
            totalWeight += GetTierWeight(cond.Tier);
        }

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeightSum = 0;
        foreach (var cond in _conditions)
        {
            currentWeightSum += GetTierWeight(cond.Tier);
            if (randomWeight < currentWeightSum)
            {
                return cond;
            }
        }

        return _conditions[0];
    }
    public ModuleActionTypeData RollRandomAction()
    {
        if (_actions == null || _actions.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        foreach (var act in _actions)
        {
            totalWeight += GetTierWeight(act.Tier);
        }

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeightSum = 0;
        foreach (var act in _actions)
        {
            currentWeightSum += GetTierWeight(act.Tier);
            if (randomWeight < currentWeightSum)
            {
                return act;
            }
        }

        return _actions[0];
    }
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ModuleConditionTypeData))]
public class ModuleConditionTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.objectReferenceValue != null)
        {
            ModuleConditionTypeData targetModule = property.objectReferenceValue as ModuleConditionTypeData;
            if (targetModule != null)
            {
                string uniqueId = string.IsNullOrEmpty(targetModule.ID) ? "NO_ID" : targetModule.ID;
                string moduleName = string.IsNullOrEmpty(targetModule.ModuleName) ? targetModule.name : targetModule.ModuleName;

                Rect labelRect = new Rect(position.x, position.y, position.width - 22, position.height);
                Rect pickerRect = new Rect(position.xMax - 20, position.y, 20, position.height);

                EditorGUI.LabelField(labelRect, $"[{uniqueId}] {moduleName}");

                property.objectReferenceValue = EditorGUI.ObjectField(pickerRect, GUIContent.none, property.objectReferenceValue, typeof(ModuleConditionTypeData), false);

                HandleDragAndDrop(position, property, typeof(ModuleConditionTypeData));
                return;
            }
        }
        EditorGUI.PropertyField(position, property, GUIContent.none);
    }

    private void HandleDragAndDrop(Rect dropArea, SerializedProperty property, System.Type requiredType)
    {
        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition)) return;

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            Object draggedObject = null;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (requiredType.IsInstanceOfType(obj))
                {
                    draggedObject = obj;
                    break;
                }
            }

            if (draggedObject != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                if (evt.type == EventType.DragPerform)
                {
                    property.objectReferenceValue = draggedObject;
                    property.serializedObject.ApplyModifiedProperties();
                    DragAndDrop.AcceptDrag();
                    evt.Use();
                }
            }
        }
    }
}

[CustomPropertyDrawer(typeof(ModuleActionTypeData))]
public class ModuleActionTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.objectReferenceValue != null)
        {
            ModuleActionTypeData targetModule = property.objectReferenceValue as ModuleActionTypeData;
            if (targetModule != null)
            {
                string uniqueId = string.IsNullOrEmpty(targetModule.ID) ? "NO_ID" : targetModule.ID;
                string moduleName = string.IsNullOrEmpty(targetModule.ModuleName) ? targetModule.name : targetModule.ModuleName;

                Rect labelRect = new Rect(position.x, position.y, position.width - 22, position.height);
                Rect pickerRect = new Rect(position.xMax - 20, position.y, 20, position.height);

                EditorGUI.LabelField(labelRect, $"[{uniqueId}] {moduleName}");

                property.objectReferenceValue = EditorGUI.ObjectField(pickerRect, GUIContent.none, property.objectReferenceValue, typeof(ModuleActionTypeData), false);

                HandleDragAndDrop(position, property, typeof(ModuleActionTypeData));
                return;
            }
        }

        EditorGUI.PropertyField(position, property, GUIContent.none);
    }

    private void HandleDragAndDrop(Rect dropArea, SerializedProperty property, System.Type requiredType)
    {
        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition)) return;

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            Object draggedObject = null;
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (requiredType.IsInstanceOfType(obj))
                {
                    draggedObject = obj;
                    break;
                }
            }

            if (draggedObject != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                if (evt.type == EventType.DragPerform)
                {
                    property.objectReferenceValue = draggedObject;
                    property.serializedObject.ApplyModifiedProperties();
                    DragAndDrop.AcceptDrag();
                    evt.Use();
                }
            }
        }
    }
}
#endif