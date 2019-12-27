using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute {
    public string controlField = "";
    public int enumCondition = -1;
    public bool hideInInspector;

    public ConditionalHideAttribute(string controlField) {
        this.controlField = controlField;
        this.hideInInspector = false;
    }

    public ConditionalHideAttribute(string controlField, bool hideInInspector) {
        this.controlField = controlField;
        this.hideInInspector = hideInInspector;
    }

    public ConditionalHideAttribute(string controlField, int enumCondition) {
        this.controlField = controlField;
        this.enumCondition = enumCondition;
        this.hideInInspector = false;
    }

    public ConditionalHideAttribute(string controlField, int enumCondition, bool hideInInspector) {
        this.controlField = controlField;
        this.enumCondition = enumCondition;
        this.hideInInspector = hideInInspector;
    }
}
