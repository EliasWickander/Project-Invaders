using System.Collections;
using System.Collections.Generic;
using ClientSidePrediction;
using UnityEngine;

public class PlayerPrediction : ClientPrediction<NetworkedPlayerInput, NetworkedPlayerState>
{
	protected override NetworkedPlayerInput GetInput(float deltaTime, uint currentTick)
	{
		Vector3 moveDirection = Vector3.zero;

		return new NetworkedPlayerInput(moveDirection, deltaTime, currentTick);
	}
}
