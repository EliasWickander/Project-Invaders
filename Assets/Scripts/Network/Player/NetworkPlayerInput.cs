using CustomToolkit.Mirror;
using UnityEngine;

//Input sent over network for player
public struct NetworkPlayerInput : INetworkClientInput
{
	public uint m_tick;
	public Vector3 m_moveDirection;
	public double m_moveTimer;

	public uint Tick => m_tick;
	public NetworkPlayerInput(uint tick, Vector3 moveDirection, double moveTimer)
	{
		m_tick = tick;
		m_moveDirection = moveDirection;
		m_moveTimer = moveTimer;
	}
}