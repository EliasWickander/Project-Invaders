namespace CustomToolkit.Mirror
{
	public interface INetworkClient
	{
		/// <summary>
		/// Last state received from the server
		/// </summary>
		INetworkClientState LatestServerState { get; }
		
		void HandleTick(uint currentTick);
	}
}