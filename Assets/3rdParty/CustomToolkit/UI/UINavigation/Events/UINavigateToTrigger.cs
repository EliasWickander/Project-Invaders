using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    public class UINavigateToTrigger : MonoBehaviour
    {
        public enum Element
        {
            Screen,
            Panel
        }
    
        public Element m_target;
        public UIScreenData m_screenData;
        public UIPanelData m_panelData;
        public bool m_openWithPanel = false;

        public void Trigger()
        {
            UINavigation uiNavigation = UINavigation.Instance;
        
            if (m_target == Element.Screen)
            {
                if (m_screenData)
                {
                    if (m_openWithPanel)
                        uiNavigation.NavigateTo(m_screenData, m_panelData);
                    else
                        uiNavigation.NavigateTo(m_screenData, null);
                }
            }
            else if (m_target == Element.Panel)
            {
                if(m_panelData)
                    uiNavigation.NavigateTo(m_panelData);
            }
        }
    }
}
