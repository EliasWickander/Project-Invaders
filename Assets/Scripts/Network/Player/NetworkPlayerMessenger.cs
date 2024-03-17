using System;
using CustomToolkit.Mirror;
using Mirror;

public class NetworkPlayerMessenger : NetworkBehaviour, INetworkClientMessenger<NetworkPlayerInput, NetworkPlayerState>
{
	public event Action<NetworkPlayerInput> OnInputReceived;

	private NetworkPlayerState m_latestServerState;
	public NetworkPlayerState LatestServerState => m_latestServerState;
	
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
		m_latestServerState = state;
	}
}