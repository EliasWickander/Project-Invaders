using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputEvent))]
public class InputEventEditor : Editor
{
    private InputEvent m_inputEvent;

    private InputAction m_selectedAction;

    private void OnEnable()
    {
        m_inputEvent = target as InputEvent;
        
        m_selectedAction = m_inputEvent.m_inputAsset.FindAction(m_inputEvent.m_selectedActionGuid);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty inputAsset = serializedObject.FindProperty(nameof(InputEvent.m_inputAsset));

        EditorGUILayout.PropertyField(inputAsset);

        if (inputAsset.objectReferenceValue != null)
        {
            InputActionAsset inputActionAsset = inputAsset.objectReferenceValue as InputActionAsset;

            if (inputActionAsset != null)
            {
                EditorGUILayout.BeginHorizontal();
            
                EditorGUILayout.PrefixLabel("Action");
            
                Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(0));

                bool activateDropDown = EditorGUILayout.DropdownButton(m_selectedAction != null ? new GUIContent($"{m_inputEvent.m_selectedActionGuid}") : new GUIContent("Nothing"), FocusType.Passive, EditorStyles.popup);
            
                EditorGUILayout.EndHorizontal();

                SerializedProperty eventType = serializedObject.FindProperty(nameof(InputEvent.m_eventType));
                EditorGUILayout.PropertyField(eventType);

                SerializedProperty e = serializedObject.FindProperty(nameof(InputEvent.m_event));
                EditorGUILayout.PropertyField(e);

                if (activateDropDown)
                {
                    DrawActions(buttonRect, inputActionAsset);
                }   
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    public void DrawActions(Rect position, InputActionAsset asset)
    {
        GenericMenu menu = new GenericMenu();
        menu.allowDuplicateNames = true;
        menu.AddItem(new GUIContent("Nothing"), m_selectedAction == null, OnActionSelected, null);
        menu.AddSeparator("");

        if (asset != null)
        {
            foreach(InputActionMap actionMap in asset.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    GUIContent actionEntry = new GUIContent($"{actionMap.name}/{action.name}");
                    menu.AddItem(actionEntry, action == m_selectedAction, OnActionSelected, action);   
                }
            }
        }
        
        menu.DropDown(position);
    }
    
    private void OnActionSelected(object action)
    {
        m_selectedAction = action as InputAction;

        m_inputEvent.m_selectedActionGuid = m_selectedAction != null ? $"{m_selectedAction.actionMap.name}/{m_selectedAction.name}" : string.Empty;

        AssetDatabase.SaveAssets();   
    }
}
