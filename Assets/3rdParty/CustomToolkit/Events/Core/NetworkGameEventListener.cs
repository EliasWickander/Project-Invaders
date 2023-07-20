using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace CustomToolkit.Events
{
    public abstract class NetworkGameEventListener<TType, TEvent> : MonoBehaviour, INetworkGameEventListener<TType> where TEvent : NetworkGameEvent<TType>
    {
        [SerializeField] 
        private ConnectionType m_listenType;

        public TEvent m_event;
        public UnityEvent<TType> m_response;
        
        private void OnEnable()
        {
            if(m_event != null)
                m_event.RegisterListener(this);
        }

        private void OnDisable()
        {
            if(m_event != null)
                m_event.UnregisterListener(this);
        }
        
        public void OnEventRaised(ConnectionType type, TType value)
        {
            if(type != m_listenType)
                return;
            
            m_response?.Invoke(value);
        }
    }
}