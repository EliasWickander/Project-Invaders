using UnityEngine;

public class NetworkPlayerPrediction : ClientPrediction<NetworkPlayerInput, NetworkPlayerState>
{
	[SerializeField] 
	private NetworkPlayer m_networkPlayer;
	
	public override NetworkPlayerInput GetInput(uint currentTick)
	{
		Player player = m_networkPlayer.Player;
		
		return new NetworkPlayerInput(currentTick, player.CurrentMoveDirection, m_networkPlayer.m_moveTimer);
	}
}