using System;

public interface INetworkClientMessenger<ClientInput, ClientState> where ClientInput : INetworkClientInput where ClientState : INetworkClientState
{
	event Action<ClientInput> OnInputReceived;

	ClientState LatestServerState { get; }

	public void SendInputToServer(ClientInput input);
	public void SendStateToClient(ClientState state);
}