using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CustomSlider), true)]
[CanEditMultipleObjects]
public class CustomSliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty handleRect = serializedObject.FindProperty("m_handleRect");
        SerializedProperty fillContainerRect = serializedObject.FindProperty("m_fillContainerRect");
        SerializedProperty fillRect = serializedObject.FindProperty("m_fillRect");
        SerializedProperty value = serializedObject.FindProperty("m_value");
        SerializedProperty minValue = serializedObject.FindProperty("m_minValue");
        SerializedProperty maxValue = serializedObject.FindProperty("m_maxValue");
        SerializedProperty onValueChanged = serializedObject.FindProperty("OnValueChanged");

        EditorGUILayout.PropertyField(handleRect, new GUIContent(handleRect.displayName, handleRect.tooltip));
        EditorGUILayout.PropertyField(fillContainerRect, new GUIContent(fillContainerRect.displayName, fillContainerRect.tooltip));
        EditorGUILayout.PropertyField(fillRect, new GUIContent(fillRect.displayName, fillRect.tooltip));
        
        EditorGUILayout.Slider(value, minValue.floatValue, maxValue.floatValue);
        
        EditorGUILayout.PropertyField(minValue, new GUIContent(minValue.displayName, minValue.tooltip));
        EditorGUILayout.PropertyField(maxValue, new GUIContent(maxValue.displayName, maxValue.tooltip));
        
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(onValueChanged, new GUIContent(onValueChanged.displayName, onValueChanged.tooltip));
        
        serializedObject.ApplyModifiedProperties();
    }
}
