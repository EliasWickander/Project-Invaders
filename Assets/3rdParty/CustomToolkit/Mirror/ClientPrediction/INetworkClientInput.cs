namespace CustomToolkit.Mirror
{
	public interface INetworkClientInput
	{
		/// <summary>
		/// The tick in which this input was sent on the client
		/// </summary>
		uint Tick { get; }
	}
}