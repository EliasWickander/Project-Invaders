using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    [CreateAssetMenu(fileName = "New Screen Data", menuName = "Project 100/UI/Screen Data")]
    public class UIScreenData : ScriptableObject
    {
        public UIScreen m_uiScreenPrefab;
        public UIPanelData[] m_panels;
    }   
}
