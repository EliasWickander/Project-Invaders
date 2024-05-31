using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CustomToolkit.Mirror
{
    [DisallowMultipleComponent]
    public class NetworkSimulation : NetworkBehaviour
    {
        public static NetworkSimulation Instance { get; private set; }
        
        public uint CurrentTick => m_currentTick;
        private uint m_currentTick = 0;
        private float m_tickTimer = 0;

        private List<INetworkClient> m_simulatedEntities = new List<INetworkClient>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            m_tickTimer += Time.deltaTime;

            while (m_tickTimer >= NetworkServer.tickInterval)
            {
                m_tickTimer -= NetworkServer.tickInterval;
                HandleTick();
                m_currentTick++;
            }
        }

        private void HandleTick()
        {
            for (int i = m_simulatedEntities.Count - 1; i >= 0; i--)
                m_simulatedEntities[i].HandleTick(CurrentTick);   
        }

        public void AddEntity(INetworkClient entity)
        {
            if(!m_simulatedEntities.Contains(entity))
                m_simulatedEntities.Add(entity);
        }

        public void RemoveEntity(INetworkClient entity)
        {
            if(m_simulatedEntities.Contains(entity))
                m_simulatedEntities.Remove(entity);
        }
    }
}
