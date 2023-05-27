using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    [CreateAssetMenu(fileName = "New Panel Data", menuName = "Custom/UI/Panel Data")]
    public class UIPanelData : ScriptableObject
    {
        public UIPanel m_uiPanelPrefab;
        public bool m_activeOnStart = false;
    }   
}
