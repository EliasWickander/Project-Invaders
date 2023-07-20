using System;
using System.Collections.Generic;
using Mirror;
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
    
    public abstract class NetworkGameEvent<T> : ScriptableObject
    {
        protected List<INetworkGameEventListener<T>> m_listeners = new List<INetworkGameEventListener<T>>();
        protected List<Action<ConnectionType, T>> m_actionListeners = new List<Action<ConnectionType, T>>();

        public void Raise(ConnectionType type, T value)
        {
            switch (type)
            {
                case ConnectionType.Server:
                    RaiseServer(value);
                    break;
                case ConnectionType.Client:
                    RaiseClient(value);
                    break;
            }
        }

        [Server]
        public void RaiseServer(T value)
        {
            for (int i = 0; i < m_listeners.Count; i++)
                m_listeners[i].OnEventRaised(ConnectionType.Server, value);
        
            for (int i = 0; i < m_actionListeners.Count; i++)
                m_actionListeners[i].Invoke(ConnectionType.Server, value);
        }

        [Client]
        public void RaiseClient(T value)
        {
            for (int i = 0; i < m_listeners.Count; i++)
                m_listeners[i].OnEventRaised(ConnectionType.Client, value);
        
            for (int i = 0; i < m_actionListeners.Count; i++)
                m_actionListeners[i].Invoke(ConnectionType.Client, value);
        }
        
        public void RegisterListener(INetworkGameEventListener<T> listener)
        {
            if(!m_listeners.Contains(listener))
                m_listeners.Add(listener);
        }

        public void RegisterListener(Action<ConnectionType, T> action)
        {
            if(!m_actionListeners.Contains(action))
                m_actionListeners.Add(action);
        }
    
        public void UnregisterListener(Action<ConnectionType, T> action)
        {
            if(m_actionListeners.Contains(action))
                m_actionListeners.Remove(action);
        }
    
        public void UnregisterListener(INetworkGameEventListener<T> listener)
        {
            if (m_listeners.Contains(listener))
                m_listeners.Remove(listener);
        }
    }
}