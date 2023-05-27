using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.Events
{
    public abstract class GameEvent : ScriptableObject
    {
        protected List<IGameEventListener> m_listeners = new List<IGameEventListener>();

        public void Raise()
        {
            for (int i = 0; i < m_listeners.Count; i++)
                m_listeners[i].OnEventRaised();
        }

        public void RegisterListener(IGameEventListener listener)
        {
            if(!m_listeners.Contains(listener))
                m_listeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            if (m_listeners.Contains(listener))
                m_listeners.Remove(listener);
        }
    }

    public abstract class GameEvent<T> : ScriptableObject
    {
        protected List<IGameEventListener<T>> m_listeners = new List<IGameEventListener<T>>();
        protected List<Action<T>> m_actionListeners = new List<Action<T>>();

        public void Raise(T value)
        {
            for (int i = 0; i < m_listeners.Count; i++)
                m_listeners[i].OnEventRaised(value);
        
            for (int i = 0; i < m_actionListeners.Count; i++)
                m_actionListeners[i].Invoke(value);
        }

        public void RegisterListener(IGameEventListener<T> listener)
        {
            if(!m_listeners.Contains(listener))
                m_listeners.Add(listener);
        }

        public void RegisterListener(Action<T> action)
        {
            if(!m_actionListeners.Contains(action))
                m_actionListeners.Add(action);
        }
    
        public void UnregisterListener(Action<T> action)
        {
            if(m_actionListeners.Contains(action))
                m_actionListeners.Remove(action);
        }
    
        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (m_listeners.Contains(listener))
                m_listeners.Remove(listener);
        }
    }
}