using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomToolkit.UI
{
    [CustomEditor(typeof(UINavigateToTrigger))]
    public class UINavigateToTriggerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
        
            SerializedProperty target = serializedObject.FindProperty(nameof(UINavigateToTrigger.m_target));

            EditorGUILayout.PropertyField(target);

            switch ((UINavigateToTrigger.Element) target.intValue)
            {
                case UINavigateToTrigger.Element.Screen:
                {
                    SerializedProperty screenData = serializedObject.FindProperty(nameof(UINavigateToTrigger.m_screenData));
                    SerializedProperty openWithPanel = serializedObject.FindProperty(nameof(UINavigateToTrigger.m_openWithPanel));
                    
                    EditorGUILayout.PropertyField(screenData);
                    EditorGUILayout.PropertyField(openWithPanel);

                    if (openWithPanel.boolValue)
                    {
                        SerializedProperty panelData = serializedObject.FindProperty(nameof(UINavigateToTrigger.m_panelData));
                
                        EditorGUILayout.PropertyField(panelData);
                    }
                    break;
                }
                case UINavigateToTrigger.Element.Panel:
                {
                    SerializedProperty panelData = serializedObject.FindProperty(nameof(UINavigateToTrigger.m_panelData));
                
                    EditorGUILayout.PropertyField(panelData);
                    break;
                }
            }
        
            serializedObject.ApplyModifiedProperties();
        }
    }   
}
