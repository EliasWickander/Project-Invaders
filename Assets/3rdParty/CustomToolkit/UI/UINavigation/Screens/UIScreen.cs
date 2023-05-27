using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    public class UIScreen : MonoBehaviour
    {
        public UIScreenData m_screenData;
        private Dictionary<UIPanelData, UIPanel> m_panels = new Dictionary<UIPanelData, UIPanel>();
        
        public UIPanel TopPanel => m_panelStack.Count > 0 ? m_panelStack.Peek() : null;
        private Stack<UIPanel> m_panelStack = new Stack<UIPanel>();
        
        public void Setup()
        {
            foreach (UIPanelData panel in m_screenData.m_panels)
            {
                if(panel.m_uiPanelPrefab == null)
                    continue;

                UIPanel panelInstance = Instantiate(panel.m_uiPanelPrefab, transform);
                panelInstance.m_panelData = panel;
                panelInstance.Setup();
                
                panelInstance.SetVisible(false);
                
                m_panels.Add(panel, panelInstance);

                if (panel.m_activeOnStart)
                {
                    NavigateTo(panel);
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < m_panelStack.Count; i++)
            {
                if (TopPanel.m_panelData.m_activeOnStart)
                    break;
                
                TopPanel.SetVisible(false);
                m_panelStack.Pop();
            }
        }
        public void SetVisible(bool visible)
        {
            if (visible == false)
            {
                Reset();
            }
            
            gameObject.SetActive(visible);
        }

        public void NavigateTo(UIPanelData panel)
        {
            UIPanel panelObject = m_panels[panel];

            if(m_panelStack.Contains(panelObject))
                return;

            m_panelStack.Push(panelObject);
            panelObject.SetVisible(true);
        }

        public void NavigateBack()
        {
            if(TopPanel == null)
                return;
            
            TopPanel.SetVisible(false);
            m_panelStack.Pop();
        }
    }    
}
