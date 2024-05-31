using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class NetworkPlayer : NetworkClient<NetworkPlayerInput, NetworkPlayerState>
{
	private Player m_player;

	public Player Player => m_player;

	public double m_moveTimer = 0;

	private NetworkPlayerState m_currentState;
	public NetworkPlayerState CurrentState => m_currentState;

	protected override void Awake()
	{
		base.Awake();
		
		m_player = GetComponent<Player>();
	}

	public override void SetState(NetworkPlayerState state)
	{
		m_player.transform.position = state.m_position;
		m_player.MoveTimer = state.m_moveTimer;
		
		PlayerTileTracker playerTileTracker = TileManager.Instance.GetTrackedTilesForPlayer(m_player.PlayerId);

		if (playerTileTracker != null)
		{
			if (state.m_trailTiles != null)
			{
				for (int i = playerTileTracker.m_trailTilePositions.Count - 1; i >= 0; i--)
				{
					if(!state.m_trailTiles.Contains(playerTileTracker.m_trailTilePositions[i]))
						PlayGrid.Instance.SetTilePendingOwner(playerTileTracker.m_trailTilePositions[i], null);
				}

				foreach (Vector2Int newTrailTile in state.m_trailTiles)
				{
					if(!playerTileTracker.m_trailTilePositions.Contains(newTrailTile))
						PlayGrid.Instance.SetTilePendingOwner(newTrailTile, m_player.PlayerId);
				}
			}

			if (state.m_ownedTiles != null)
			{
				for (int i = playerTileTracker.m_ownedTilePositions.Count - 1; i >= 0; i--)
				{
					if(!state.m_ownedTiles.Contains(playerTileTracker.m_ownedTilePositions[i]))
						PlayGrid.Instance.SetTileOwner(playerTileTracker.m_ownedTilePositions[i], null);
				}

				foreach (Vector2Int newOwnedTile in state.m_ownedTiles)
				{
					if(!playerTileTracker.m_ownedTilePositions.Contains(newOwnedTile))
						PlayGrid.Instance.SetTileOwner(newOwnedTile, m_player.PlayerId);
				}
			}
		}

		m_currentState = state;
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

		return GetState(input.Tick);
	}

	public NetworkPlayerState GetState(uint tick)
	{
		Vector2Int[] trailTiles = Array.Empty<Vector2Int>();
		Vector2Int[] ownedTiles = Array.Empty<Vector2Int>();
		
		PlayerTileTracker playerTileTracker = TileManager.Instance.GetTrackedTilesForPlayer(m_player.PlayerId);

		if (playerTileTracker != null)
		{
			trailTiles = playerTileTracker.m_trailTilePositions.ToArray();
			ownedTiles = playerTileTracker.m_ownedTilePositions.ToArray();
		}
		else
		{
			Debug.Log("no tiles");
		}
		
		return new NetworkPlayerState()
		{
			m_tick = tick,
			m_position = transform.position,
			m_moveTimer = m_moveTimer,
			m_trailTiles = trailTiles,
			m_ownedTiles = ownedTiles
		};	
	}
}
