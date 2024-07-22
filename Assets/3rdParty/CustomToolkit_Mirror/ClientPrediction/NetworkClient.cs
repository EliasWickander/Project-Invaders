using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace CustomToolkit.Mirror
{
	[RequireComponent(typeof(NetworkIdentity))]
	public abstract class NetworkClient<ClientInput, ClientState> : MonoBehaviour, INetworkClient where ClientInput : INetworkClientInput where ClientState : INetworkClientState
	{
		public INetworkClientState LatestServerState => m_messenger.LatestServerState;

		[SerializeField] 
		private ClientPrediction<ClientInput, ClientState> m_prediction;
		public ClientPrediction<ClientInput, ClientState> Prediction => m_prediction;

		private INetworkClientMessenger<ClientInput, ClientState> m_messenger;
		public INetworkClientMessenger<ClientInput, ClientState> Messenger => m_messenger;
	
		protected NetworkIdentity m_identity = null;
		private Queue<ClientInput> m_inputQueue = new Queue<ClientInput>(6);

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
			
			ClearBuffers();
			NetworkSimulation.Instance.AddEntity(this);
		}

		protected virtual void OnDisable()
		{
			if(m_messenger != null)
				m_messenger.OnInputReceived -= OnInputReceived;
			
			NetworkSimulation.Instance.RemoveEntity(this);
		}

		public virtual void HandleTick(uint currentTick)
		{
			//Process all inputs on server
			if (m_identity.isServer)
				ProcessInputs();
			
			if (m_identity.isClient && m_identity.isOwned)
				m_prediction.HandleTick(currentTick, m_messenger.LatestServerState);
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
			m_messenger.SendStateToClient(GameClient.Instance.NetworkTransmissionManager.GetNextTransmissionId(), state);
		}
	
		public void SendInputToServer(ClientInput input)
		{
			m_messenger.SendInputToServer(input);
		}
	
		private void OnInputReceived(ClientInput input)
		{
			//Ignore obsolete input
			if(input.Tick < LatestServerState.Tick)
				return;

			m_inputQueue.Enqueue(input);
		}
	
		public abstract void SetState(ClientState state);
		public abstract ClientState ProcessInput(ClientInput input);
		
		public void ClearBuffers()
		{
			m_inputQueue.Clear();

			m_prediction?.ClearBuffers();
		}

		public void SetActive(bool isActive)
		{
			enabled = isActive;
				
			if(m_prediction != null)
				m_prediction.enabled = isActive;
		}

		//Reset network simulation to start from new server state
		public void ForceSyncServerState(ClientState state)
		{
			Messenger.LatestServerState = state;
			
			m_prediction?.ForceSyncServerState(state);
		}
	}
}
