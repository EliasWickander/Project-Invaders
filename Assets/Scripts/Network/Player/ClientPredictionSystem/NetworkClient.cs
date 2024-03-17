using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public abstract class NetworkClient<ClientInput, ClientState> : MonoBehaviour, INetworkClient where ClientInput : INetworkClientInput where ClientState : INetworkClientState
{
	public INetworkClientState LatestServerState => m_messenger.LatestServerState;
	public uint CurrentTick => m_currentTick;

	[SerializeField] 
	private ClientPrediction<ClientInput, ClientState> m_prediction;

	[SerializeField] 
	private INetworkClientMessenger<ClientInput, ClientState> m_messenger;
	
	private NetworkIdentity m_identity = null;
	private Queue<ClientInput> m_inputQueue = new Queue<ClientInput>(6);
	
	private uint m_currentTick = 0;
	private float m_tickTimer = 0;

	private void Awake()
	{
		m_identity = GetComponent<NetworkIdentity>();
		m_prediction = GetComponent<ClientPrediction<ClientInput, ClientState>>();
		m_messenger = GetComponent<INetworkClientMessenger<ClientInput, ClientState>>();
	}

	private void OnEnable()
	{
		m_messenger.OnInputReceived += OnInputReceived;
	}

	private void OnDisable()
	{
		m_messenger.OnInputReceived -= OnInputReceived;
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
		//Process all inputs on server
		if (m_identity.isServer)
			ProcessInputs();

		if (m_identity.isClient && m_identity.isOwned)
			m_prediction.HandleTick(m_currentTick, m_messenger.LatestServerState);
	}

	private void ProcessInputs()
	{
		if(m_inputQueue.Count <= 0)
			return;
	
		//Process inputs
		ClientState lastRecordedState = default(ClientState);
		
		while (m_inputQueue.Count > 0)
		{
			ClientInput input = m_inputQueue.Dequeue();
			
			ClientState state = ProcessInput(input);

			m_prediction.RecordState(input.Tick, state);

			lastRecordedState = state;
		}

		//If we processed input, send new state to client
		SendStateToClient(lastRecordedState);
	}
	
	private void SendStateToClient(ClientState state)
	{
		m_messenger.SendStateToClient(state);
	}
	
	public void SendInputToServer(ClientInput input)
	{
		m_messenger.SendInputToServer(input);
	}
	
	private void OnInputReceived(ClientInput input)
	{
		m_inputQueue.Enqueue(input);
	}
	
	public abstract void SetState(ClientState state);
	public abstract ClientState ProcessInput(ClientInput input);
}
