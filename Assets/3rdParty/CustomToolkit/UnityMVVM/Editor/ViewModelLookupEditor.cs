using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomToolkit.UnityMVVM.Internal;
using UnityEditor;
using UnityEngine;
using UnityWeld_Editor;

namespace CustomToolkit.UnityMVVM
{
    [CustomEditor(typeof(ViewModelLookup))]
    public class ViewModelLookupEditor : BaseBindingEditor
    {
        private ViewModelLookup m_targetScript;
        
        private void OnEnable()
        {
            m_targetScript = target as ViewModelLookup;
        }

        public override void OnInspectorGUI()
        {
            if (CannotModifyInPlayMode())
            {
                GUI.enabled = false;
            }
            
            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = defaultLabelStyle;

            m_targetScript.m_searchMode = (ViewModelLookup.SearchMode)EditorGUILayout.EnumPopup("Search Mode", m_targetScript.m_searchMode);

            string[] viewModelTypeNames = MVVMHelper.GetAllViewModelTypes().Select(type => type.ToString()).ToArray();
            
            ShowViewModelMenu(
                new GUIContent("View Model"),
                viewModelTypeNames,
                updatedValue => m_targetScript.m_viewModelTypeName = updatedValue,
                m_targetScript.m_viewModelTypeName);
            
            EditorStyles.label.fontStyle = defaultLabelStyle;
        }
    }
}
