using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public abstract class ClientPrediction<ClientInput, ClientState> : MonoBehaviour where ClientInput : INetworkClientInput where ClientState : INetworkClientState
{
	[SerializeField] 
	private bool m_debug = false;
	
	[SerializeField, Tooltip("The number of ticks that can be stored in input/state buffers")] 
	private uint m_bufferSize = 1024;

	[SerializeField] 
	private NetworkClient<ClientInput, ClientState> m_client;
	
	private ClientState[] m_stateBuffer;
	private ClientInput[] m_inputBuffer;
	private ClientState m_lastProcessedState;

	private NetworkIdentity m_identity = null;
	
	private void Awake()
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

		//Store input for this frame
		ClientInput currentInput = GetInput(currentTick);
		
		m_inputBuffer[bufferIndex] = currentInput;
		
		//Store state for this frame
		m_stateBuffer[bufferIndex] = m_client.ProcessInput(currentInput);
		
		if(!m_identity.isServer) 
			m_client.SendInputToServer(currentInput);	
	}

	private void HandleServerReconciliation(uint currentTick, ClientState latestServerState)
	{
		m_lastProcessedState = latestServerState;

		uint serverStateBufferIndex = latestServerState.Tick % m_bufferSize;

		//If latest server state doesn't match the state we expect this tick, we're out of sync. Reconcile
		if (!latestServerState.Equals(m_stateBuffer[serverStateBufferIndex]))
		{
			if (m_debug)
				Debug.Log("Reconciling", gameObject);
			
			//Rewind
			m_client.SetState(latestServerState);

			//Update buffer at index of latest server state
			m_stateBuffer[serverStateBufferIndex] = latestServerState;

			//Re-simulate the rest of the ticks up to current tick on client
			uint tickToProcess = latestServerState.Tick + 1;

			while (tickToProcess < currentTick)
			{
				uint bufferIndex = tickToProcess % m_bufferSize;
			
				ClientState stateToProcess = m_client.ProcessInput(m_inputBuffer[bufferIndex]);
				
				//Update buffer with recalculated state
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
}
