using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Mirror;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class NetworkPlayerTiles : NetworkClient<NetworkPlayerTilesInput, NetworkPlayerTilesState>
{
    private Player m_player;

    public Player Player => m_player;

    protected override void Awake()
    {
        base.Awake();
        
        m_player = GetComponent<Player>();
    }

    public override void SetState(NetworkPlayerTilesState state)
    {
        var playGrid = PlayGrid.Instance;

        foreach (var ownedTile in state.m_ownedTiles)
        {
            playGrid.SetTileOwner(ownedTile, m_player.PlayerId);
        }

        foreach (var pendingTile in state.m_pendingTiles)
        {
            playGrid.SetTilePendingOwner(pendingTile, m_player.PlayerId);
        }
    }

    public override NetworkPlayerTilesState ProcessInput(NetworkPlayerTilesInput input)
    {
        return new NetworkPlayerTilesState
        {
            m_tick = input.Tick,
            m_ownedTiles = input.m_ownedTiles,
            m_pendingTiles = input.m_pendingTiles
        };
    }
}
