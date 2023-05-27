using UnityEngine;
using UnityEngine.Events;

namespace CustomToolkit.Events
{
    public class BoolToUnityEvent : MonoBehaviour
    {
        public GameEvent<bool> m_event;

        public UnityEvent m_onTrue;
        public UnityEvent m_onFalse;
        private void OnEnable()
        {
            if(m_event)
                m_event.RegisterListener(OnEventInvoked);
        }

        private void OnDisable()
        {
            if(m_event)
                m_event.UnregisterListener(OnEventInvoked);
        }

        private void OnEventInvoked(bool value)
        {
            if(value)
                m_onTrue?.Invoke();
            else
                m_onFalse?.Invoke();
        }
    }   
}
