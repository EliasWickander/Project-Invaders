using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class ClientPrediction : NetworkBehaviour
{
	private const float c_serverTickRate = 30f;
	private const int c_bufferSize = 1024;
	
	private float m_minTimeBetweenTicks;
	private int m_currentTick;
	private float m_timer;

	private NetworkPlayerState[] m_stateBuffer;
	private NetworkPlayerInput[] m_inputBuffer;
	private NetworkPlayerState m_latestServerState;
	private NetworkPlayerState m_lastProcessedState;

	private NetworkIdentity m_networkIdentity;

	private Queue<NetworkPlayerInput> m_inputQueue;

	private float m_horizontalInput;
	private float m_verticalInput;
	
	private void Awake()
	{
		m_networkIdentity = GetComponent<NetworkIdentity>();
		m_inputQueue = new Queue<NetworkPlayerInput>();
	}

	private void Start()
	{
		m_minTimeBetweenTicks = 1f / c_serverTickRate;
		
		m_stateBuffer = new NetworkPlayerState[c_bufferSize];
		m_inputBuffer = new NetworkPlayerInput[c_bufferSize];
	}

	private void Update()
	{
		m_horizontalInput = Input.GetAxis("Horizontal");
		m_verticalInput = Input.GetAxis("Vertical");
		
		m_timer += Time.deltaTime;

		while (m_timer >= m_minTimeBetweenTicks)
		{
			m_timer -= m_minTimeBetweenTicks;
			HandleTick();
			m_currentTick++;
		}
	}

	private void HandleTick()
	{
		if (m_networkIdentity.isServer)
		{
			ProcessInputQueue();
		}

		if (m_networkIdentity.isOwned)
		{
			if (!m_networkIdentity.isServer)
			{
				if (!m_latestServerState.Equals(default(NetworkPlayerState)) &&
				    (m_lastProcessedState.Equals(default(NetworkPlayerState)) ||
				     !m_latestServerState.Equals(m_lastProcessedState)))
				{
					HandleServerReconciliation();
				}
			}
		
			int bufferIndex = m_currentTick % c_bufferSize;

			//Store input for this frame
			NetworkPlayerInput currentInput = new NetworkPlayerInput();
			currentInput.m_tick = m_currentTick;
			currentInput.m_input = new Vector3(m_horizontalInput, 0, m_verticalInput);
			m_inputBuffer[bufferIndex] = currentInput;
		
			//Store state for this frame
			m_stateBuffer[bufferIndex] = ProcessInput(currentInput);
		
			if(!m_networkIdentity.isServer) 
				SendInputToServer(currentInput);	
		}
	}

	private void HandleServerReconciliation()
	{
		m_lastProcessedState = m_latestServerState;

		int serverStateBufferIndex = m_latestServerState.m_tick % c_bufferSize;
		float positionError = Vector3.Distance(m_latestServerState.m_position, m_stateBuffer[serverStateBufferIndex].m_position);

		if (positionError > 0.001f)
		{
			//Rewind
			transform.position = m_latestServerState.m_position;

			//Update buffer at index of latest server state
			m_stateBuffer[serverStateBufferIndex] = m_latestServerState;

			//Re-simulate the rest of the ticks up to current tick on client
			int tickToProcess = m_latestServerState.m_tick + 1;

			while (tickToProcess < m_currentTick)
			{
				int bufferIndex = tickToProcess % c_bufferSize;
				
				//Process new movement with reconciled state
				NetworkPlayerState stateToProcess = ProcessInput(m_inputBuffer[bufferIndex]);
				
				//Update buffer with recalculated state
				m_stateBuffer[bufferIndex] = stateToProcess;

				tickToProcess++;
			}
		}
	}

	private NetworkPlayerState ProcessInput(NetworkPlayerInput input)
	{
		//Need to multiply with min time between ticks to make sure it's same on server
		transform.position += input.m_input * 5 * m_minTimeBetweenTicks;

		return new NetworkPlayerState()
		{
			m_tick = input.m_tick,
			m_position = transform.position
		};
	}

	[Command(channel = Channels.Unreliable)]	
	private void SendInputToServer(NetworkPlayerInput input)
	{
		m_inputQueue.Enqueue(input);
	}

	[ClientRpc(channel = Channels.Unreliable)]
	private void SendStateToClient(NetworkPlayerState state)
	{
		OnReceivedStateFromServer(state);
	}

	private void OnReceivedStateFromServer(NetworkPlayerState state)
	{
		m_latestServerState = state;
	}
	
	private void ProcessInputQueue()
	{
		//Process inputs
		int bufferIndex = -1;
		while (m_inputQueue.Count > 0)
		{
			NetworkPlayerInput input = m_inputQueue.Dequeue();

			bufferIndex = input.m_tick % c_bufferSize;

			NetworkPlayerState state = ProcessInput(input);
			m_stateBuffer[bufferIndex] = state;
		}

		//If we processed input, send new state to client
		if (bufferIndex != -1)
		{
			SendStateToClient(m_stateBuffer[bufferIndex]);
		}
	}
}
