using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.UI
{
    public class UINavigateBackTrigger : MonoBehaviour
    {
        public enum Element
        {
            Both,
            Screen,
            Panel
        }
    
        public Element m_target;

        public void Trigger()
        {
            UINavigation uiNavigation = UINavigation.Instance;
        
            switch(m_target)
            {
                case Element.Both:
                {
                    uiNavigation.NavigateBack();
                    break;
                }
                case Element.Screen:
                {
                    uiNavigation.NavigateBackScreen();
                    break;
                }
                case Element.Panel:
                {
                    uiNavigation.NavigateBackPanel();
                    break;
                }
            }
        }
    }   
}
