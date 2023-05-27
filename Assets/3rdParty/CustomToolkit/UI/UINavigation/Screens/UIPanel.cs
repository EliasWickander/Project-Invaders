using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    public class UIPanel : MonoBehaviour
    {
        public UIPanelData m_panelData;

        public void Setup()
        {
        
        }
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }   
}
