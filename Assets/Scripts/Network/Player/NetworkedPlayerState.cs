using System;
using System.Collections.Generic;
using ClientSidePrediction;
using UnityEngine;

public struct NetworkedPlayerState : IEquatable<NetworkedPlayerState>, INetworkedClientState
{
	public uint m_lastProcessedInputTick;
	public uint LastProcessedInputTick => m_lastProcessedInputTick;

	public List<Vector2Int> m_pendingTiles;

	public NetworkedPlayerState(List<Vector2Int> pendingTiles, uint lastProcessedInputTick)
	{
		m_pendingTiles = pendingTiles;
		m_lastProcessedInputTick = lastProcessedInputTick;
	}
	
	public bool Equals(NetworkedPlayerState other)
	{
		return m_pendingTiles == other.m_pendingTiles;
	}

	public bool Equals(INetworkedClientState other)
	{
		return other is NetworkedPlayerState _other && Equals(_other);
	}
}
