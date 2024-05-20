using System;
using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkPlayer))]
public class NetworkPlayerTilesPrediction : ClientPrediction<NetworkPlayerTilesInput, NetworkPlayerTilesState>
{
    private NetworkPlayer m_player;
    
    protected override void Awake()
    {
        base.Awake();

        m_player = GetComponent<NetworkPlayer>();
    }

    private void OnEnable()
    {
        m_player.Player.OnDeadEvent += OnPlayerDead;
    }

    private void OnDisable()
    {
        m_player.Player.OnDeadEvent -= OnPlayerDead;
    }

    public override NetworkPlayerTilesInput GetInput(uint currentTick)
    {
        var tileManager = TileManager.Instance;

        PlayerTileTracker playerTileTracker = tileManager.GetTrackedTilesForPlayer(m_player.Player.PlayerId);

        if (playerTileTracker == null)
            return new NetworkPlayerTilesInput();
        
        return new NetworkPlayerTilesInput(currentTick, playerTileTracker.m_ownedTilePositions.ToArray(), playerTileTracker.m_trailTilePositions.ToArray());
    }
    
    private void OnPlayerDead()
    {
        //ClearBuffers();
    }
}
