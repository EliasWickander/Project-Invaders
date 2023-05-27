using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnClientErrorEventListener))]
[CanEditMultipleObjects]
public class OnClientErrorEventListenerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.ApplyModifiedProperties();

        SerializedProperty onClientErrorEvent = serializedObject.FindProperty(nameof(OnClientErrorEventListener.m_event));
        SerializedProperty singleErrorListener = serializedObject.FindProperty(nameof(OnClientErrorEventListener.m_singleErrorListener));
        SerializedProperty onClientErrorResponse = serializedObject.FindProperty(nameof(OnClientErrorEventListener.m_response));

        EditorGUILayout.PropertyField(onClientErrorEvent);
        EditorGUILayout.PropertyField(singleErrorListener);
        
        if (singleErrorListener.boolValue)
        {
            SerializedProperty errorTypeToListenFor = serializedObject.FindProperty(nameof(OnClientErrorEventListener.m_errorTypeToListenFor));
            EditorGUILayout.PropertyField(errorTypeToListenFor);
        }
        
        EditorGUILayout.PropertyField(onClientErrorResponse);
        serializedObject.ApplyModifiedProperties();
    }
}
