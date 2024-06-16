using System.Collections.Generic;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

//Player state handled over network
public struct NetworkPlayerState : INetworkClientState, NetworkMessage
{
	public uint m_tick;
	public Vector3 m_position;
	public double m_moveTimer;

	public Vector2Int[] m_ownedTiles;
	public Vector2Int[] m_trailTiles;
	
	public uint Tick => m_tick;

	public bool Equals(NetworkPlayerState other)
	{
		return Vector3.Distance(other.m_position, m_position) < 0.001f && 
		       m_moveTimer.Equals(other.m_moveTimer);
	}
	
	public bool Equals(INetworkClientState other)
	{
		return other is NetworkPlayerState _other && Equals(_other);
	}
	
	public string Log()
	{
		return $"Tick: {m_tick}\n" +
		       $"Position: {m_position}\n" +
		       $"Move Timer: {m_moveTimer}\n" +
		       $"Owned Tiles count: {m_ownedTiles?.Length ?? 0}\n" +
		       $"Trail Tiles count: {m_trailTiles?.Length ?? 0}";
	}
}