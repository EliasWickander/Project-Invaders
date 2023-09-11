using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerTileTracker
{
    public string m_playerId;
    public List<WorldGridTile> m_ownedTiles = new List<WorldGridTile>();
    public List<WorldGridTile> m_trailTiles = new List<WorldGridTile>();
}

public class TileManager : MonoBehaviour
{
    [SerializeField] 
    private Server_OnTileSteppedOnEvent m_onTileSteppedOnServerEvent;
    
    [SerializeField] 
    private Client_OnTileSteppedOnEvent m_onTileSteppedOnClientEvent;

    [SerializeField]
    private Server_OnTileStatusChangedEvent m_onTileStatusChangedServerEvent;

    [SerializeField]
    private Client_OnTileStatusChangedEvent m_onTileStatusChangedClientEvent;
    
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
        
        if(m_onTileStatusChangedServerEvent != null)
            m_onTileStatusChangedServerEvent.RegisterListener(OnTileStatusChangedServer);
        
        if(m_onTileStatusChangedClientEvent != null)
            m_onTileStatusChangedClientEvent.RegisterListener(OnTileStatusChangedClient);
        
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
        
        if (m_playerTileTrackers.TryGetValue(removedPlayerId, out var tileTracker))
        {
            if (NetworkServer.active && !e.m_player.isOwned)
            {
                //Remove all trail tiles
                List<WorldGridTile> trailTiles = tileTracker.m_trailTiles;
				
                for(int i = trailTiles.Count - 1; i >= 0; i--)
                {
                    WorldGridTile trailTile = trailTiles[i];
                    
                    TileStatus oldStatus = trailTile.TileStatus;
					TileStatus newStatus = new TileStatus() {PendingOwnerPlayerId = null, OwnerPlayerId = oldStatus.OwnerPlayerId};
                    
                    m_grid.SetTileStatus(trailTile.m_gridPos, newStatus);
                }		
                
                //Remove all owned tiles
                List<WorldGridTile> ownedTiles = tileTracker.m_ownedTiles;

                for(int i = ownedTiles.Count - 1; i >= 0; i--)
                {
                    WorldGridTile ownedTile = ownedTiles[i];
                    
                    TileStatus oldStatus = ownedTile.TileStatus;
                    TileStatus newStatus = new TileStatus() {PendingOwnerPlayerId = oldStatus.PendingOwnerPlayerId, OwnerPlayerId = null};
                    
                    m_grid.SetTileStatus(ownedTile.m_gridPos, newStatus);
                }
            }

            m_playerTileTrackers.Remove(removedPlayerId);
        }
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
    
    public void OnTileSteppedOnServer(OnTileSteppedOnEventData data)
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
                    FillEnclosedTiles(data.m_player.PlayerId, loop);   
                }
            }
        }
    }

    private void FillEnclosedTiles(string playerId, List<WorldGridTile> loop)
    {
        if (!m_playerTileTrackers.TryGetValue(playerId, out var tileTracker))
            return;
        
        //If loop is the same as trail, we failed to enclose a loop. Just fill trail tiles
        if (loop.Count == tileTracker.m_trailTiles.Count)
        {
            foreach (var trailTile in loop)
                m_grid.SetTileOwner(trailTile.m_gridPos, playerId);
            
            return;
        }
        
        //If loop is not the same length as trail, that means we have an actual enclosed loop. Fill all nodes enclosed by this loop
        Dictionary<int, List<WorldGridTile>> potentialConnections = new Dictionary<int, List<WorldGridTile>>();
        
        foreach (WorldGridTile tile in loop)
        {
            WorldGridTile upNeighbour = m_grid.GetNeighbour(tile, Vector2Int.up);
            WorldGridTile downNeighbour = m_grid.GetNeighbour(tile, Vector2Int.down);

            if ((upNeighbour != null && loop.Contains(upNeighbour)) || (downNeighbour != null && loop.Contains(downNeighbour)))
            {
                if (!potentialConnections.ContainsKey(tile.m_gridPos.y))
                    potentialConnections.Add(tile.m_gridPos.y, new List<WorldGridTile>() {tile});
                else
                    potentialConnections[tile.m_gridPos.y].Add(tile);
            }
        }
        
        //Fill all enclosed nodes
        foreach (KeyValuePair<int, List<WorldGridTile>> tentativeConnection in potentialConnections)
        {
            (WorldGridTile, WorldGridTile) xConnection = FindFurthestTiles(tentativeConnection.Value);
            
            WorldGridTile first = xConnection.Item1;
            WorldGridTile second = xConnection.Item2;

            Vector2Int fillDirection = second.m_gridPos.x > first.m_gridPos.x ? Vector2Int.right : Vector2Int.left;
            
            WorldGridTile current = m_grid.GetNeighbour(first, fillDirection);
            
            while (current != null && current != second)
            {
                m_grid.SetTileOwner(current.m_gridPos, playerId);

                current = m_grid.GetNeighbour(current, fillDirection);
            }
        }

        //All enclosed nodes are filled. Now fill trail loop
        foreach (var trailTile in loop)
        {
            m_grid.SetTileOwner(trailTile.m_gridPos, playerId);
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
     
    private (WorldGridTile, WorldGridTile) FindFurthestTiles(List<WorldGridTile> tiles)
    {
        if (tiles == null || tiles.Count <= 0)
            return (null, null);
        
        if (tiles.Count == 1)
            return (tiles[0], tiles[0]);

        if (tiles.Count == 2)
            return (tiles[0], tiles[1]);
        
        WorldGridTile first = null;
        WorldGridTile second = null;
        int maxDistance = 0;

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                int xDelta = Mathf.Abs(tiles[i].m_gridPos.x - tiles[j].m_gridPos.x);
                if (xDelta > maxDistance)
                {
                    maxDistance = xDelta;
                    first = tiles[i];
                    second = tiles[j];
                }
            }
        }

        return (first, second);
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