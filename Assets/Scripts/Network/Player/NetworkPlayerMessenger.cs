using System;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

public class NetworkPlayerMessenger : NetworkBehaviour, INetworkClientMessenger<NetworkPlayerInput, NetworkPlayerState>
{
	public event Action<NetworkPlayerInput> OnInputReceived;

	public NetworkPlayerState LatestServerState { get; set; }

	public void SendInputToServer(NetworkPlayerInput input)
	{
		CmdSendInputToServer(input);
	}

	public void SendStateToClient(NetworkPlayerState state)
	{
		if (ServerDebug.s_debugPackages)
			Debug.Log($"Sending package to client: {state.Log()}");	
		
		RpcSendStateToClient(state);
	}

	[Command(channel = Channels.Unreliable)]
	private void CmdSendInputToServer(NetworkPlayerInput input)
	{
		OnInputReceived?.Invoke(input);
	}
	
	[ClientRpc(channel = Channels.Unreliable)]
	private void RpcSendStateToClient(NetworkPlayerState state)
	{
		if (ServerDebug.s_debugPackages)
			Debug.Log($"Received package on client: {state.Log()}");	
		
		//Ignore outdated server states
		if (state.Tick <= LatestServerState.Tick)
		{
			if (ServerDebug.s_debugPackages)
				Debug.Log($"Ignored package due to being out of date. Tick: {state.Tick}. Latest server tick: {LatestServerState.Tick}");	
			
			return;	
		}

		LatestServerState = state;
	}
}