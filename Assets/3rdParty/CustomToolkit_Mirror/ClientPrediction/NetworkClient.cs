using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CustomToolkit.Mirror
{
	[RequireComponent(typeof(NetworkIdentity))]
	public abstract class NetworkClient<ClientInput, ClientState> : MonoBehaviour, INetworkClient where ClientInput : INetworkClientInput where ClientState : INetworkClientState
	{
		public INetworkClientState LatestServerState => m_messenger.LatestServerState;
		public uint CurrentTick => m_currentTick;

		[SerializeField] 
		private ClientPrediction<ClientInput, ClientState> m_prediction;

		private INetworkClientMessenger<ClientInput, ClientState> m_messenger;
	
		private NetworkIdentity m_identity = null;
		private Queue<ClientInput> m_inputQueue = new Queue<ClientInput>(6);
	
		private uint m_currentTick = 0;
		private float m_tickTimer = 0;

		protected virtual void Awake()
		{
			m_identity = GetComponent<NetworkIdentity>();
			m_prediction = GetComponent<ClientPrediction<ClientInput, ClientState>>();
			
			if(m_prediction == null)
				Debug.LogError("No prediction component was found on this object. Please add a component of type ClientPrediction", gameObject);
			
			m_messenger = GetComponent<INetworkClientMessenger<ClientInput, ClientState>>();
			
			if(m_messenger == null)
				Debug.LogError("No messenger was found on this object. Please add a component of type INetworkClientMessenger", gameObject);
		}

		protected virtual void OnEnable()
		{
			if(m_messenger != null)
				m_messenger.OnInputReceived += OnInputReceived;
		}

		protected virtual void OnDisable()
		{
			if(m_messenger != null)
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
			else if(!m_identity.isServer)
				HandleOtherPlayerState(m_messenger.LatestServerState);
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
	
		private void HandleOtherPlayerState(ClientState state)
		{
			m_prediction.RecordState(m_messenger.LatestServerState.Tick, m_messenger.LatestServerState);
			
			//TODO: State interpolation for smoother update
			SetState(state);
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
}
