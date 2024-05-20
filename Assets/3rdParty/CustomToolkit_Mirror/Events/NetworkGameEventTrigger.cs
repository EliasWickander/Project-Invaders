using UnityEngine;

namespace CustomToolkit.Mirror.Events
{
    public abstract class NetworkGameEventTrigger<TType, TEvent> : MonoBehaviour, INetworkGameEventTrigger where TEvent : NetworkGameEvent<TType>
    {
        public TEvent m_event;
    
        public TType m_value;
    
        public virtual void Trigger()
        {
            if(m_event != null)
                m_event.Raise(ConnectionType.Client, m_value);
        }
    }
}