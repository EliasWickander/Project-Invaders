using CustomToolkit.Mirror;
using UnityEngine;

//Player state handled over network
public struct NetworkPlayerState : INetworkClientState
{
	public uint m_tick;
	public Vector3 m_position;
	public double m_moveTimer;

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
}