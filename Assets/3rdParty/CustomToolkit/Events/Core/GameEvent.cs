using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CustomToolkit.Events
{
    public abstract class GameEvent : ScriptableObject
    {
	    public abstract void ClearListeners();
    }

    public abstract class GameEvent<T> : GameEvent
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

        public override void ClearListeners()
        {
	        m_listeners.Clear();
	        m_actionListeners.Clear();
        }
    }
}