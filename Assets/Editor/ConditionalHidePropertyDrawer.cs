using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        ConditionalHideAttribute controlAttribute = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideResult(controlAttribute, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;

        if (!controlAttribute.hideInInspector || enabled) {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        ConditionalHideAttribute controlAttribute = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideResult(controlAttribute, property);

        if (!controlAttribute.hideInInspector || enabled) {
            return EditorGUI.GetPropertyHeight(property, label);
        } else {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionalHideResult(ConditionalHideAttribute controlAttribute, SerializedProperty property) {
        bool enabled = true;
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, controlAttribute.controlField);
        SerializedProperty controlProperty = property.serializedObject.FindProperty(conditionPath);

        if (controlProperty != null) {
            if (controlAttribute.enumCondition != -1) {
                enabled = controlProperty.enumValueIndex == controlAttribute.enumCondition;
            } else {
                enabled = controlAttribute.enableWhenTrue == controlProperty.boolValue;
            }
        } else {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + controlAttribute.controlField);
        }

        return enabled;
    }
}
