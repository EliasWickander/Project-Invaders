using CustomToolkit.Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkPlayer))]
public class NetworkPlayerPrediction : ClientPrediction<NetworkPlayerInput, NetworkPlayerState>
{
	private NetworkPlayer m_player;

	protected override void Awake()
	{
		base.Awake();

		m_player = GetComponent<NetworkPlayer>();
	}

	public override NetworkPlayerInput GetInput(uint currentTick)
	{
		Player player = m_player.Player;
		
		return new NetworkPlayerInput {
			m_tick = currentTick, 
			m_moveDirection = player.CurrentMoveDirection, 
			m_moveTimer = m_player.m_moveTimer};
	}
}