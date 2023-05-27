using UnityEngine;

namespace CustomToolkit.Events
{
    public abstract class GameEventTrigger<TEvent> : MonoBehaviour, IGameEventTrigger where TEvent : GameEvent
    {
        public TEvent m_event;

        public virtual void Trigger()
        {
            if(m_event != null)
                m_event.Raise();
        }
    }

    public abstract class GameEventTrigger<TType, TEvent> : MonoBehaviour, IGameEventTrigger where TEvent : GameEvent<TType>
    {
        public TEvent m_event;
    
        public TType m_value;
    
        public virtual void Trigger()
        {
            if(m_event != null)
                m_event.Raise(m_value);
        }
    }
}