using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute {
    public string controlField = "";
    public bool enableWhenTrue;
    public int enumCondition = -1;
    public bool hideInInspector;

    public ConditionalHideAttribute(string controlField, bool enableWhenTrue, bool hideInInspector) {
        this.controlField = controlField;
        this.enableWhenTrue = enableWhenTrue;
        this.hideInInspector = hideInInspector;
    }

    public ConditionalHideAttribute(string controlField, int enumCondition, bool hideInInspector) {
        this.controlField = controlField;
        this.enumCondition = enumCondition;
        this.hideInInspector = hideInInspector;
    }
}
