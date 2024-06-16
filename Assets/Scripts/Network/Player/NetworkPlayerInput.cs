using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

//Input sent over network for player
public struct NetworkPlayerInput : INetworkClientInput, NetworkMessage
{
	public uint m_tick;
	public Vector3 m_moveDirection;
	public double m_moveTimer;

	public uint Tick => m_tick;
	
	public string Log()
	{
		return $"Tick: {m_tick}\n" +
		       $"Move Direction: {m_moveDirection}\n" +
		       $"Move Timer: {m_moveTimer}\n";
	}
}