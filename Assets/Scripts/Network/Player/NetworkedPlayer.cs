using System.Collections;
using System.Collections.Generic;
using ClientSidePrediction;
using UnityEngine;

public class NetworkedPlayer : NetworkedClient<NetworkedPlayerInput, NetworkedPlayerState>
{
	[SerializeField] 
	private Player m_player;
	
	public override void SetState(NetworkedPlayerState state)
	{
		m_player.PendingTiles = state.m_pendingTiles;
	}

	public override void ProcessInput(NetworkedPlayerInput input)
	{
		
	}

	protected override NetworkedPlayerState RecordState(uint lastProcessedInputTick)
	{
		return new NetworkedPlayerState(m_player.PendingTiles, lastProcessedInputTick);
	}
}
