using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomToolkit.UnityMVVM
{
    public class ViewModelLookup : MonoBehaviour, IViewModelProvider
    {
        public enum SearchMode
        {
            UpInHierarchy,
            FullScene
        }
        
        public SearchMode m_searchMode;

        public string m_viewModelTypeName;

        private Type m_viewModelType;
        private Object m_viewModel;

        public object GetViewModel()
        {
            if (m_viewModel != null)
                return m_viewModel;

            if (m_viewModel == null && !string.IsNullOrEmpty(m_viewModelTypeName))
                m_viewModelType = Type.GetType(m_viewModelTypeName);
            
            switch (m_searchMode)
            {
                case SearchMode.UpInHierarchy:
                {
                    m_viewModel = GetComponentInParent(m_viewModelType);
                    break;
                }
                case SearchMode.FullScene:
                {
                    m_viewModel = FindObjectOfType(m_viewModelType);
                    break;
                }
            }

            return m_viewModel;
        }

        public string GetViewModelTypeName()
        {
            return m_viewModelTypeName;
        }
    }
}
