using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;

public class PlayerTileTracker
{
    public string m_playerId;
    public List<WorldGridTile> m_ownedTiles = new List<WorldGridTile>();
    public List<WorldGridTile> m_trailTiles = new List<WorldGridTile>();
}

public class TileManager : Singleton<TileManager>
{
    [SerializeField]
    private Server_OnTileSteppedOnEvent m_onTileSteppedOnServerEvent;

    [SerializeField]
    private Client_OnTileSteppedOnEvent m_onTileSteppedOnClientEvent;

    [SerializeField]
    private Server_OnTileStatusChangedEvent m_onTileStatusChangedServerEvent;

    [SerializeField]
    private Client_OnTileStatusChangedEvent m_onTileStatusChangedClientEvent;

    [SerializeField]
    private Server_OnPlayerSpawnedEvent m_onPlayerSpawnedServerEvent;

    [SerializeField]
    private Client_OnPlayerSpawnedEvent m_onPlayerSpawnedClientEvent;
    
    [SerializeField]
    private Server_OnPlayerKilledEvent m_onPlayerKilledServerEvent;

    private PlayGrid m_grid;

    private Dictionary<string, PlayerTileTracker> m_playerTileTrackers = new Dictionary<string, PlayerTileTracker>();

    private void Start()
    {
        m_grid = PlayGrid.Instance;
    }

    private void OnEnable()
    {
        if(m_onTileSteppedOnServerEvent != null)
            m_onTileSteppedOnServerEvent.RegisterListener(OnTileSteppedOnServer);

        if(m_onTileSteppedOnClientEvent != null)
	        m_onTileSteppedOnClientEvent.RegisterListener(OnTileSteppedOnClient);
        
        if(m_onTileStatusChangedServerEvent != null)
            m_onTileStatusChangedServerEvent.RegisterListener(OnTileStatusChangedServer);

        if(m_onTileStatusChangedClientEvent != null)
            m_onTileStatusChangedClientEvent.RegisterListener(OnTileStatusChangedClient);

        if(m_onPlayerSpawnedServerEvent != null)
            m_onPlayerSpawnedServerEvent.RegisterListener(OnPlayerSpawnedServer);

        if(m_onPlayerSpawnedClientEvent != null)
	        m_onPlayerSpawnedClientEvent.RegisterListener(OnPlayerSpawnedClient);
        
        if(m_onPlayerKilledServerEvent != null)
            m_onPlayerKilledServerEvent.RegisterListener(OnPlayerKilled);

        GameWorld.OnPlayerAddedEvent += OnPlayerAdded;
        GameWorld.OnPlayerRemovedEvent += OnPlayerRemoved;
    }

    private void OnDisable()
    {
        if(m_onTileSteppedOnServerEvent != null)
            m_onTileSteppedOnServerEvent.UnregisterListener(OnTileSteppedOnServer);

        if(m_onTileStatusChangedServerEvent != null)
            m_onTileStatusChangedServerEvent.UnregisterListener(OnTileStatusChangedServer);

        if(m_onTileStatusChangedClientEvent != null)
            m_onTileStatusChangedClientEvent.UnregisterListener(OnTileStatusChangedClient);

        if(m_onPlayerSpawnedServerEvent != null)
            m_onPlayerSpawnedServerEvent.UnregisterListener(OnPlayerSpawnedServer);

        if(m_onPlayerSpawnedClientEvent != null)
	        m_onPlayerSpawnedClientEvent.UnregisterListener(OnPlayerSpawnedClient);
        
        if(m_onPlayerKilledServerEvent != null)
            m_onPlayerKilledServerEvent.UnregisterListener(OnPlayerKilled);

        GameWorld.OnPlayerAddedEvent -= OnPlayerAdded;
        GameWorld.OnPlayerRemovedEvent -= OnPlayerRemoved;
    }

    private void OnPlayerAdded(object sender, PlayerAddedEventArgs e)
    {
        string addedPlayerId = e.m_player.PlayerId;

        m_playerTileTrackers.TryAdd(addedPlayerId, new PlayerTileTracker() {m_playerId = addedPlayerId});
    }

    private void OnPlayerRemoved(object sender, PlayerRemovedEventArgs e)
    {
        string removedPlayerId = e.m_player.PlayerId;

        if (NetworkServer.active && !e.m_player.isOwned)
            ClearAssociatedTiles(removedPlayerId);

        if(m_playerTileTrackers.ContainsKey(removedPlayerId))
            m_playerTileTrackers.Remove(removedPlayerId);
    }

    private void ClearAssociatedTiles(string playerId)
    {
        if (m_playerTileTrackers.TryGetValue(playerId, out var tileTracker))
        {
            //Remove all trail tiles
            List<WorldGridTile> trailTiles = tileTracker.m_trailTiles;

            for (int i = trailTiles.Count - 1; i >= 0; i--)
            {
                WorldGridTile trailTile = trailTiles[i];

                TileStatus oldStatus = trailTile.TileStatus;
                TileStatus newStatus = new TileStatus()
                    { PendingOwnerPlayerId = null, OwnerPlayerId = oldStatus.OwnerPlayerId };

                m_grid.SetTileStatus(trailTile.m_gridPos, newStatus);
            }

            //Remove all owned tiles
            List<WorldGridTile> ownedTiles = tileTracker.m_ownedTiles;

            for (int i = ownedTiles.Count - 1; i >= 0; i--)
            {
                WorldGridTile ownedTile = ownedTiles[i];

                TileStatus oldStatus = ownedTile.TileStatus;
                TileStatus newStatus = new TileStatus()
                    { PendingOwnerPlayerId = oldStatus.PendingOwnerPlayerId, OwnerPlayerId = null };

                m_grid.SetTileStatus(ownedTile.m_gridPos, newStatus);
            }
        }
    }

    [Server]
    private void OnPlayerSpawnedServer(OnPlayerSpawnedGameEventData data)
    {
        Player spawnedPlayer = data.m_player;
        Transform spawnTransform = spawnedPlayer.SpawnTransform;
        WorldGridTile spawnTile = spawnedPlayer.SpawnTile;

        NetworkManagerCustom.Instance.DisableStartPoint(spawnTransform);

        SetOwnerArea(spawnedPlayer, spawnTile, data.m_startTerritoryRadius);
    }

    private void OnPlayerSpawnedClient(OnPlayerSpawnedGameEventData data)
    {
	    Player spawnedPlayer = data.m_player;
	    WorldGridTile spawnTile = spawnedPlayer.SpawnTile;
	    
	    SetOwnerArea(spawnedPlayer, spawnTile, data.m_startTerritoryRadius);
    }
    
    [Server]
    private void OnPlayerKilled(OnPlayerKilledGameEventData data)
    {
        Player killedPlayer = data.m_player;
        NetworkManagerCustom.Instance.EnableStartPoint(killedPlayer.SpawnTransform);

        ClearAssociatedTiles(killedPlayer.PlayerId);
    }

    private void SetOwnerArea(Player player, WorldGridTile sourceTile, int radius)
    {
        List<WorldGridTile> nodesWithinRadius = GetNodesWithinRadius(sourceTile, radius);

        foreach (WorldGridTile tile in nodesWithinRadius)
            PlayGrid.Instance.SetTileOwner(tile.m_gridPos, player.PlayerId);
    }

    public List<WorldGridTile> GetNodesWithinRadius(WorldGridTile sourceTile, int radius)
    {
        List<WorldGridTile> nodesWithinRadius = new List<WorldGridTile>();
        Queue<(WorldGridTile node, int distance)> queue = new Queue<(WorldGridTile, int)>();
        HashSet<WorldGridTile> visited = new HashSet<WorldGridTile>();

        queue.Enqueue((sourceTile, 0));
        visited.Add(sourceTile);

        while (queue.Count > 0)
        {
            (WorldGridTile currentNode, int distance) = queue.Dequeue();
            nodesWithinRadius.Add(currentNode);

            if (distance < radius)
            {
                foreach (WorldGridTile neighborNode in m_grid.GetNeighbours(currentNode))
                {
                    if (!visited.Contains(neighborNode))
                    {
                        visited.Add(neighborNode);
                        queue.Enqueue((neighborNode, distance + 1));
                    }
                }
            }
        }

        return nodesWithinRadius;
    }

    private void OnTileStatusChangedClient(OnTileStatusChangedEventData data)
    {
        OnTileStatusChanged(data);
    }

    private void OnTileStatusChangedServer(OnTileStatusChangedEventData data)
    {
        OnTileStatusChanged(data);
    }

    private void OnTileStatusChanged(OnTileStatusChangedEventData data)
    {
        WorldGridTile tile = data.m_tile;
        TileStatus oldStatus = data.m_oldStatus;
        TileStatus newStatus = data.m_newStatus;

        string oldPendingOwnerId = oldStatus.PendingOwnerPlayerId;
        string newPendingOwnerId = newStatus.PendingOwnerPlayerId;

        string oldOwnerId = oldStatus.OwnerPlayerId;
        string newOwnerId = newStatus.OwnerPlayerId;

        //There is a new pending owner
        if (oldPendingOwnerId != newPendingOwnerId)
        {
            //Remove old trail tile association
            if (!string.IsNullOrEmpty(oldPendingOwnerId))
            {
                if (m_playerTileTrackers.TryGetValue(oldPendingOwnerId, out var tileTracker))
                {
                    if (tileTracker.m_trailTiles.Contains(tile))
                        tileTracker.m_trailTiles.Remove(tile);
                }
            }

            //Add new trail tile association
            if (!string.IsNullOrEmpty(newPendingOwnerId))
            {
                if (m_playerTileTrackers.TryGetValue(newPendingOwnerId, out var tileTracker))
                {
                    if (!tileTracker.m_trailTiles.Contains(tile))
                        tileTracker.m_trailTiles.Add(tile);
                }
            }
        }

        //There is a new owner
        if (oldOwnerId != newOwnerId)
        {
            //Remove old owned tile association
            if (!string.IsNullOrEmpty(oldOwnerId))
            {
                if (m_playerTileTrackers.TryGetValue(oldOwnerId, out var tileTracker))
                {
                    if (tileTracker.m_ownedTiles.Contains(tile))
                        tileTracker.m_ownedTiles.Remove(tile);
                }
            }

            //Add new owned tile association
            if (!string.IsNullOrEmpty(newOwnerId))
            {
                if (m_playerTileTrackers.TryGetValue(newOwnerId, out var tileTracker))
                {
                    if (!tileTracker.m_ownedTiles.Contains(tile))
                        tileTracker.m_ownedTiles.Add(tile);
                }
            }
        }
    }

    private void OnTileSteppedOnClient(OnTileSteppedOnEventData data)
    {
	    if (data.m_player == null)
	    {
		    Debug.LogError("Player can not be null in OnTileSteppedOnEventData", gameObject);
		    return;
	    }

	    WorldGridTile tile = m_grid.GetNode(data.m_tilePos.x, data.m_tilePos.y);
	    string playerId = data.m_player.PlayerId;

	    if (tile.TileStatus.OwnerPlayerId != playerId)
	    {
		    //If walking on own trail,
		    if (tile.TileStatus.PendingOwnerPlayerId == playerId)
			    return;

		    m_grid.SetTilePendingOwner(tile.m_gridPos, playerId);
	    }
	    else
	    {
		    //If walking on tile that's owned, add all nodes enclosed by trail to owned tiles
		    if (m_playerTileTrackers.TryGetValue(playerId, out var tileTracker))
		    {
			    if (tileTracker.m_trailTiles.Count > 0)
			    {
				    Player player = GameClient.Instance.GameWorld.GetPlayerFromId(playerId);

				    EncloseLoop(data.m_player.PlayerId, tile,player.m_lastOwnedTileSteppedOn, tileTracker.m_trailTiles, out List<WorldGridTile> loop);

				    FillEnclosedArea(data.m_player.PlayerId, loop);
			    }
		    }
	    }
    }
    
    public void OnTileSteppedOnServer(OnTileSteppedOnEventData data)
    {

    }

    /// <summary>
    /// Fill the enclosed area (set ownership) of tiles as defined by loop
    /// </summary>
    /// <param name="playerId">Player that will own the filled tiles</param>
    /// <param name="loop">Loop that defines the enclosed area</param>
    private void FillEnclosedArea(string playerId, List<WorldGridTile> loop)
    {
        GetBoundsFromLoop(loop, out Vector2Int min, out Vector2Int max);

        //Add spacing by one to include potential edge nodes
        min.x--;
        min.y--;
        max.x++;
        max.y++;

        List<WorldGridTile> nodesToFill = m_grid.GetNodesInBounds(min, max, true);

        //Get all nodes that we don't want to fill
        List<WorldGridTile> floodedNodes = new List<WorldGridTile>();
        Flood(min, min, max, loop, ref floodedNodes);

        //Subtract all nodes in bounds with nodes we don't want to fill
        foreach (WorldGridTile floodedNode in floodedNodes)
            nodesToFill.Remove(floodedNode);

        //Fill the rest
        foreach (WorldGridTile nodeToFill in nodesToFill)
            m_grid.SetTileOwner(nodeToFill.m_gridPos, playerId);
    }

    /// <summary>
    /// Use flood fill algorithm to get all nodes within bounds but outside of enclosed area
    /// </summary>
    /// <param name="startPos">Start position of flood fill</param>
    /// <param name="boundsMin">Min bounds</param>
    /// <param name="boundsMax">Max bounds</param>
    /// <param name="loop">Enclosed area loop</param>
    /// <param name="floodedNodes">Result</param>
    /// <returns>Did flood succeed?</returns>
    private bool Flood(Vector2Int startPos, Vector2Int boundsMin, Vector2Int boundsMax, List<WorldGridTile> loop, ref List<WorldGridTile> floodedNodes)
    {
        Queue<WorldGridTile> queue = new Queue<WorldGridTile>();

        WorldGridTile startNode = m_grid.GetNode(startPos.x, startPos.y);

        if (startNode == null)
        {
            Debug.LogError("Flood fill failed. Start node was null");
            return false;
        }

        floodedNodes.Add(startNode);
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            WorldGridTile current = queue.Dequeue();

            List<WorldGridTile> neighbours = m_grid.GetNeighbours(current, true, true);

            foreach (WorldGridTile neighbour in neighbours)
            {
                if (neighbour.m_gridPos.x < boundsMin.x || neighbour.m_gridPos.y < boundsMin.y || neighbour.m_gridPos.x > boundsMax.x || neighbour.m_gridPos.y > boundsMax.y)
                    continue;

                if (loop.Contains(neighbour))
                    continue;

                if (floodedNodes.Contains(neighbour))
                    continue;

                floodedNodes.Add(neighbour);
                queue.Enqueue(neighbour);
            }
        }

        return true;
    }

    /// <summary>
    /// Get bounds that contain a loop of tiles
    /// </summary>
    /// <param name="loop">Loop</param>
    /// <param name="min">Min bounds</param>
    /// <param name="max">Max bounds</param>
    private void GetBoundsFromLoop(List<WorldGridTile> loop, out Vector2Int min, out Vector2Int max)
    {
        min = new Vector2Int(int.MaxValue, int.MaxValue);
        max = new Vector2Int(int.MinValue, int.MinValue);

        foreach (WorldGridTile tile in loop)
        {
            if (tile.m_gridPos.x < min.x)
                min.x = tile.m_gridPos.x;

            if (tile.m_gridPos.y < min.y)
                min.y = tile.m_gridPos.y;

            if (tile.m_gridPos.x > max.x)
                max.x = tile.m_gridPos.x;

            if (tile.m_gridPos.y > max.y)
                max.y = tile.m_gridPos.y;
        }
    }

    private void EncloseLoop(string playerId, WorldGridTile startTile, WorldGridTile endTile, List<WorldGridTile> trail, out List<WorldGridTile> loop)
    {
        loop = new List<WorldGridTile>(trail);

        if (!m_playerTileTrackers.TryGetValue(playerId, out var tileTracker))
            return;

        Pathfinding pathfinding = GameClient.Instance.GameWorld.Pathfinding;
        List<PathNode> path = pathfinding.FindPath(tileTracker.m_ownedTiles, startTile, endTile);

        if (path != null)
        {
            foreach (PathNode pathNode in path)
            {
                loop.Add(pathNode.m_tile);
            }
        }
    }

    public void ChangeTileOwnership(string oldPlayerId, string newPlayerId)
    {
	    List<WorldGridTile> tilesToGive = GetOwnedTiles(oldPlayerId);

	    for (int i = tilesToGive.Count - 1; i >= 0; i--)
		    m_grid.SetTileOwner(tilesToGive[i].m_gridPos, newPlayerId);
    }
    
    public List<WorldGridTile> GetOwnedTiles(string playerId)
    {
        if (!m_playerTileTrackers.TryGetValue(playerId, out var tileTracker))
            return null;

        return tileTracker.m_ownedTiles;
    }

    public List<WorldGridTile> GetTrailTiles(string playerId)
    {
        if (!m_playerTileTrackers.TryGetValue(playerId, out var tileTracker))
            return null;

        return tileTracker.m_trailTiles;
    }

}
