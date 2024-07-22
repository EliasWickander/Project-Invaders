using System;
using Mirror;
using UnityEngine;

namespace CustomToolkit.Mirror
{
	[RequireComponent(typeof(NetworkIdentity))]
	public abstract class ClientPrediction<ClientInput, ClientState> : MonoBehaviour
		where ClientInput : INetworkClientInput where ClientState : INetworkClientState
	{
		[SerializeField, Tooltip("The number of ticks that can be stored in input/state buffers")]
		private uint m_bufferSize = 1024;

		[SerializeField]
		private NetworkClient<ClientInput, ClientState> m_client;

		private ClientState[] m_stateBuffer;
		private ClientInput[] m_inputBuffer;
		private ClientState m_lastProcessedState;

		private NetworkIdentity m_identity = null;
		
		protected virtual void Awake()
		{
			m_identity = GetComponent<NetworkIdentity>();

			m_stateBuffer = new ClientState[m_bufferSize];
			m_inputBuffer = new ClientInput[m_bufferSize];
		}

		public void HandleTick(uint currentTick, ClientState latestServerState)
		{
			if (!m_identity.isServer)
			{
				if (latestServerState != null && (m_lastProcessedState == null || !m_lastProcessedState.Equals(latestServerState)))
					HandleServerReconciliation(currentTick, latestServerState);
			}

			uint bufferIndex = currentTick % m_bufferSize;

			// Store input for this frame
			ClientInput currentInput = GetInput(currentTick);
			m_inputBuffer[bufferIndex] = currentInput;

			m_client.SendInputToServer(currentInput);

			if (!m_identity.isServer)
			{
				// Store state for this frame
				m_stateBuffer[bufferIndex] = m_client.ProcessInput(currentInput);
			}
		}
		
		public void HandleServerReconciliation(uint currentTick, ClientState latestServerState)
		{
			if (currentTick < latestServerState.Tick)
			{
				if (GameDebug.s_debugNetworkMessages)
					Debug.LogWarning("Current tick is less than the latest server state tick, which is not valid.");
				
				return;
			}

			m_lastProcessedState = latestServerState;

			uint serverStateBufferIndex = latestServerState.Tick % m_bufferSize;

			// If latest server state doesn't match the state we expect this tick, we're out of sync. Reconcile
			if (!latestServerState.Equals(m_stateBuffer[serverStateBufferIndex]))
			{
				if (GameDebug.s_debugNetworkMessages)
				{
					Debug.Log("Reconciling. Latest server state was " + latestServerState.Log() + "\n Predicted state was " + m_stateBuffer[serverStateBufferIndex].Log());
				}

				// Rewind
				m_client.SetState(latestServerState);

				// Update buffer at index of latest server state
				m_stateBuffer[serverStateBufferIndex] = latestServerState;

				// Re-simulate the rest of the ticks up to current tick on client
				uint tickToProcess = latestServerState.Tick + 1;

				while (tickToProcess < currentTick) // Note: <= to include current tick
				{
					uint bufferIndex = tickToProcess % m_bufferSize;

					ClientState stateToProcess = m_client.ProcessInput(m_inputBuffer[bufferIndex]);

					// Update buffer with recalculated state
					m_stateBuffer[bufferIndex] = stateToProcess;

					tickToProcess++;
				}
			}
		}

		public void RecordState(uint inputTick, ClientState state)
		{
			uint bufferIndex = inputTick % m_bufferSize;

			m_stateBuffer[bufferIndex] = state;
		}
		
		public abstract ClientInput GetInput(uint currentTick);

		public void ClearBuffers()
		{
			m_lastProcessedState = default;
			
			if(m_stateBuffer != null)
				Array.Clear(m_stateBuffer, 0, m_stateBuffer.Length);
			
			if(m_inputBuffer != null)
				Array.Clear(m_inputBuffer, 0, m_inputBuffer.Length);
		}

		public void ForceSyncServerState(ClientState state)
		{
			RecordState(state.Tick, state);
			
			m_client.SetState(state);
		}
	}
}