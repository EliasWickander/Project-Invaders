using System.Collections.Generic;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class NetworkPlayer : NetworkClient<NetworkPlayerInput, NetworkPlayerState>
{
	private Player m_player;

	public Player Player => m_player;

	public double m_moveTimer = 0;

	protected override void Awake()
	{
		base.Awake();

		m_player = GetComponent<Player>();
	}

	public override void SetState(NetworkPlayerState state)
	{
		m_player.transform.position = state.m_position;
		m_player.MoveTimer = state.m_moveTimer;
	}

	public override NetworkPlayerState ProcessInput(NetworkPlayerInput input)
	{
		if (input.m_moveTimer >= m_player.PlayerData.MoveSpeed)
		{
			if (input.m_moveDirection != Vector3.zero)
			{
				m_player.Move(input.m_moveDirection);

				m_moveTimer = 0;	
			}
		}
		else
		{
			m_moveTimer += NetworkServer.tickInterval;
		}

		return new NetworkPlayerState()
		{
			m_tick = input.m_tick,
			m_position = transform.position,
			m_moveTimer = m_moveTimer
		};	
	}
}
