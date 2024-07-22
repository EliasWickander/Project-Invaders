using System;

namespace CustomToolkit.Mirror
{
	public interface INetworkClientMessenger<ClientInput, ClientState> where ClientInput : INetworkClientInput where ClientState : INetworkClientState
	{
		event Action<ClientInput> OnInputReceived;

		ClientState LatestServerState { get; set; }

		public void SendInputToServer(ClientInput input);
		public void SendStateToClient(int transmissionId, ClientState state);
	}
}