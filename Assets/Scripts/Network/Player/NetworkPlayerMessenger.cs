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
		//Ignore outdated server states
		if(state.Tick <= LatestServerState.Tick)
			return;

		LatestServerState = state;
	}
}