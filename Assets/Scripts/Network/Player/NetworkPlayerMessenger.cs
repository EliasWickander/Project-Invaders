using System;
using CustomToolkit.Mirror;
using UnityEngine;

public class NetworkPlayerMessenger : NetworkClientMessenger, INetworkClientMessenger<NetworkPlayerInput, NetworkPlayerState>
{
	public event Action<NetworkPlayerInput> OnInputReceived;

	public NetworkPlayerState LatestServerState { get; set; }

	public void SendInputToServer(NetworkPlayerInput input)
	{
		if (GameDebug.s_debugNetworkMessages)
			Debug.Log($"Sending input package to server: {input.Log()}");
		
		byte[] inputBuffer = Serialize<NetworkPlayerInput>(input);
		
		SendBytesToServer(inputBuffer);
	}

	public void SendStateToClient(int transmissionId, NetworkPlayerState state)
	{
		if (GameDebug.s_debugNetworkMessages)
			Debug.Log($"Sending state package to client: {state.Log()}");
		
		byte[] stateBuffer = Serialize<NetworkPlayerState>(state);

		StartCoroutine(SendBytesToClient(transmissionId, stateBuffer));
	}
	
	protected override void OnReceivedMessageOnClient(byte[] messageBuffer)
	{
		NetworkPlayerState receivedState = Deserialize<NetworkPlayerState>(messageBuffer);
		
		if (GameDebug.s_debugNetworkMessages)
			Debug.Log($"Received state package on client: {receivedState.Log()}");	
		
		//Ignore outdated server states
		if (receivedState.Tick <= LatestServerState.Tick)
		{
			if (GameDebug.s_debugNetworkMessages)
				Debug.Log($"Ignored package due to being out of date. Tick: {receivedState.Tick}. Latest server tick: {LatestServerState.Tick}");	
			
			return;	
		}

		LatestServerState = receivedState;
	}

	protected override void OnReceivedMessageOnServer(byte[] messageBuffer)
	{
		NetworkPlayerInput receivedInput = Deserialize<NetworkPlayerInput>(messageBuffer);
		
		OnInputReceived?.Invoke(receivedInput);
	}
}