using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    [CreateAssetMenu(fileName = "New Panel Data", menuName = "Project 100/UI/Panel Data")]
    public class UIPanelData : ScriptableObject
    {
        public UIPanel m_uiPanelPrefab;
        public bool m_activeOnStart = false;
    }   
}
