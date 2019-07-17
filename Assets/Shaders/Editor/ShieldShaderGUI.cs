// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using UnityEngine;
using UnityEditor;

public class ShieldShaderGUI : ShaderGUI
{
    private static class Styles
    {
        public static GUIContent uvSetLabel = new GUIContent("UV Set");

        public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
        public static GUIContent rimPowerText = new GUIContent("Rim Power", "");
    }
    
    MaterialProperty albedoMap = null;
    MaterialProperty albedoColor = null;
    MaterialProperty rimPower = null;

    MaterialEditor m_MaterialEditor;

    public void FindProperties(MaterialProperty[] props)
    {
        albedoMap = FindProperty("_MainTex", props);
        albedoColor = FindProperty("_Color", props);
        rimPower = FindProperty("_RimPower", props);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        ShaderPropertiesGUI(material);
    }

    public void ShaderPropertiesGUI(Material material)
    {
        // Use default labelWidth
        EditorGUIUtility.labelWidth = 0f;

        // Detect any changes to the material
        EditorGUI.BeginChangeCheck();
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);

            m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
            m_MaterialEditor.ShaderProperty(rimPower, Styles.rimPowerText);
        }
        EditorGUI.EndChangeCheck();

        EditorGUILayout.Space();
        
        m_MaterialEditor.RenderQueueField();
    }
    

    static void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
    }
}
