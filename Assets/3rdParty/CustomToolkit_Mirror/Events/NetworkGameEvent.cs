using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum ConnectionType
{
    Server,
    Client,
}

namespace CustomToolkit.Mirror.Events
{
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
            if (!m_listeners.Contains(listener))
                m_listeners.Add(listener);
        }

        public void RegisterListener(Action<ConnectionType, T> action)
        {
            if (!m_actionListeners.Contains(action))
                m_actionListeners.Add(action);
        }

        public void UnregisterListener(Action<ConnectionType, T> action)
        {
            if (m_actionListeners.Contains(action))
                m_actionListeners.Remove(action);
        }

        public void UnregisterListener(INetworkGameEventListener<T> listener)
        {
            if (m_listeners.Contains(listener))
                m_listeners.Remove(listener);
        }
    }
}