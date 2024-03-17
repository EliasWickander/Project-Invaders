using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

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

public class NetworkPlayer : NetworkClient<NetworkPlayerInput, NetworkPlayerState>
{
	[SerializeField]
	private Player m_player;

	public Player Player => m_player;

	private NetworkIdentity m_networkIdentity;

	private Queue<NetworkPlayerInput> m_inputQueue;

	public double m_moveTimer = 0;

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
