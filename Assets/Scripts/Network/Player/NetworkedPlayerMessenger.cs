using System;
using System.Collections;
using System.Collections.Generic;
using ClientSidePrediction;
using Mirror;
using UnityEngine;

public class NetworkedPlayerMessenger : NetworkBehaviour, INetworkedClientMessenger<NetworkedPlayerInput, NetworkedPlayerState>
{
	public event Action<NetworkedPlayerInput> OnInputReceived;
	
	public NetworkedPlayerState m_latestServerState;
	public NetworkedPlayerState LatestServerState => m_latestServerState;
	
	public void SendState(NetworkedPlayerState state)
	{
		RpcSendState(state);
	}

	[ClientRpc(channel = Channels.Unreliable)]
	private void RpcSendState(NetworkedPlayerState state)
	{
		m_latestServerState = state;
	}

	public void SendInput(NetworkedPlayerInput input)
	{
		CmdSendInput(input);
	}

	[Command(channel = Channels.Unreliable)]
	private void CmdSendInput(NetworkedPlayerInput input)
	{
		OnInputReceived?.Invoke(input);
	}
}
