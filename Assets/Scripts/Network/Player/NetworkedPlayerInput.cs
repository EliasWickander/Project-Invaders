using ClientSidePrediction;
using UnityEngine;

public struct NetworkedPlayerInput : INetworkedClientInput
{
	public float m_deltaTime;
	public uint m_tick;
	public Vector3 m_moveDirection;
	
	public float DeltaTime => m_deltaTime;
	public uint Tick => m_tick;

	public NetworkedPlayerInput(Vector3 moveDirection, float deltaTime, uint tick)
	{
		m_moveDirection = moveDirection;
		m_deltaTime = deltaTime;
		m_tick = tick;
	}
}
