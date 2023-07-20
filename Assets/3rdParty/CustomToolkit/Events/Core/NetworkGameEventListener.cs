using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace CustomToolkit.Events
{
    public abstract class NetworkGameEventListener<TType, TEvent> : MonoBehaviour, IGameEventListener<TType> where TEvent : GameEvent<TType>
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

        public virtual void OnEventRaised(TType value)
        {
            if(!IsEnabled())
                return;
            
            m_response?.Invoke(value);
        }

        /// <summary>
        /// Is connection of the type this event is listening for
        /// </summary>
        /// <returns></returns>
        private bool IsEnabled()
        {
            NetworkIdentity netIdentity = GameClient.Instance.netIdentity;

            switch (m_listenType)
            {
                case ConnectionType.Server:
                    return netIdentity.isServer;
                case ConnectionType.Client:
                    return netIdentity.isLocalPlayer;
                case ConnectionType.Both:
                    return netIdentity.isServer || netIdentity.isLocalPlayer;
            }

            return false;
        }
    }
}