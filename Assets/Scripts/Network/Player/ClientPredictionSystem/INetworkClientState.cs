using System;

public interface INetworkClientState : IEquatable<INetworkClientState>
{
	/// <summary>
	/// The tick in which this state was processed
	/// </summary>
	uint Tick { get; }
}