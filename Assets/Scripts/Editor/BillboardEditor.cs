using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Billboard))]
public class BillboardEditor : Editor
{
    Billboard billboard;
    SerializedObject GetTarget;
    bool changeOccured = false;

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        changeOccured = false;

        billboard = (Billboard)target;
        GetTarget = new SerializedObject(billboard);
        GetTarget.Update();

        // CONSTRAINTS
        SerializedProperty constraint = GetTarget.FindProperty("axisConstraint");

        bool constraintValue = constraint.boolValue;
        constraintValue = EditorGUILayout.ToggleLeft(constraint.displayName, constraintValue);
        if (constraintValue != constraint.boolValue) changeOccured = true;
        constraint.boolValue = constraintValue;

        int indentLevel = EditorGUI.indentLevel;

        if (constraintValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(GetTarget.FindProperty("reference"));
            EditorGUILayout.PropertyField(GetTarget.FindProperty("lockedAxis"));
            EditorGUI.indentLevel--;
        }

        // CONSTANT SIZE
        SerializedProperty constantSize = GetTarget.FindProperty("constantScreenSize");

        bool constantSizeValue = constantSize.boolValue;
        constantSizeValue = EditorGUILayout.ToggleLeft(constantSize.displayName, constantSizeValue);
        if (constantSizeValue != constantSize.boolValue) changeOccured = true;
        constantSize.boolValue = constantSizeValue;

        if (constantSizeValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(GetTarget.FindProperty("scaleOffset"));
            EditorGUILayout.PropertyField(GetTarget.FindProperty("sizeCoef"));
            EditorGUI.indentLevel--;
        }

        if (changeOccured)
        {
            EditorUtility.SetDirty(target);
        }
        GetTarget.ApplyModifiedProperties();

        EditorGUI.indentLevel = indentLevel;
    }
}
