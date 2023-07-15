using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomToolkit.UI
{
    public class UINavigation : MonoBehaviour
    {
        public static UINavigation Instance { get; private set; }

        public UIScreenData[] m_defaultScreens;

        private Dictionary<UIScreenData, UIScreen> m_screens = new Dictionary<UIScreenData, UIScreen>();

        public UIScreen ActiveScreen => m_screenStack.Count > 0 ? m_screenStack.Peek() : null;
        private Stack<UIScreen> m_screenStack = new Stack<UIScreen>();


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Init();
            }
        }

        private void Init()
        {
            foreach (UIScreenData screen in m_defaultScreens)
            {
                if(screen == null || screen.m_uiScreenPrefab == null)
                    continue;

                CreateScreen(screen);
            }

            if (m_screens.Count > 0)
            {
                NavigateTo(m_screens.Values.First().m_screenData);
            }
        }

        public UIScreen CreateScreen(UIScreenData screenData)
        {
            if (m_screens.ContainsKey(screenData))
            {
                Debug.LogError("Trying to create screen with data that's already used for another screen");
                return null;
            }
            
            UIScreen screenInstance = Instantiate(screenData.m_uiScreenPrefab, transform);
            screenInstance.m_screenData = screenData;
            screenInstance.Setup();
                
            screenInstance.SetVisible(false);
                
            m_screens.Add(screenData, screenInstance);

            return screenInstance;
        }

        public void NavigateTo(UIScreenData screen, UIPanelData panel = null)
        {
            if (screen == null || screen.m_uiScreenPrefab == null)
            {
                Debug.LogError("Tried to navigate to null screen");
                return;
            }

            UIScreen screenObject = m_screens.ContainsKey(screen) ? m_screens[screen] : CreateScreen(screen); 
            
            if (ActiveScreen)
            {
                ActiveScreen.SetVisible(false);
            }
            
            m_screenStack.Push(screenObject);

            ActiveScreen.SetVisible(true);
            
            if(panel != null)
                NavigateTo(panel);
        }

        public void NavigateTo(UIPanelData panel)
        {
            if (panel == null || panel.m_uiPanelPrefab == null)
            {
                Debug.LogError("Tried to navigate to null panel");
                return;
            }

            if (ActiveScreen == null)
            {
                Debug.LogError("Tried to navigate to panel but active screen is null");
                return;
            }
            
            if (!ActiveScreen.m_screenData.m_panels.Contains(panel))
            {
                Debug.LogError("Tried to navigate to panel that isn't associated with active screen");
                return;
            }
            
            ActiveScreen.NavigateTo(panel);
        }

        public void NavigateBack()
        {
            if (ActiveScreen == null)
            {
                Debug.LogError("Tried to navigate back but active screen is null");
                return;
            }

            if (ActiveScreen.TopPanel != null)
            {
                ActiveScreen.NavigateBack();   
            }
            else
            {
                NavigateBackScreen();
            }
        }
        
        public void NavigateBackPanel()
        {
            if (ActiveScreen == null)
            {
                Debug.LogError("Tried to navigate back but active screen is null");
                return;
            }
            
            if (ActiveScreen.TopPanel != null)
            {
                ActiveScreen.NavigateBack();   
            }
        }

        public void NavigateBackScreen()
        {
            if (m_screenStack.Count > 1)
            {
                ActiveScreen.SetVisible(false);
                    
                m_screenStack.Pop();
                    
                ActiveScreen.SetVisible(true);
            }
        }
        
        public void Reset()
        {
            if (ActiveScreen != null)
            {
                ActiveScreen.SetVisible(false);
                m_screenStack.Clear();
            }
        }
    }
}