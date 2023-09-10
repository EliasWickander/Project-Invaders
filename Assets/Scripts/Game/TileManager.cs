using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

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
    
    private Dictionary<string, List<WorldGridTile>> m_ownedTiles = new Dictionary<string, List<WorldGridTile>>();
    private Dictionary<string, List<WorldGridTile>> m_trailTiles = new Dictionary<string, List<WorldGridTile>>();
    
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
        if(!m_trailTiles.ContainsKey(e.m_player.PlayerId))
            m_trailTiles.Add(e.m_player.PlayerId, new List<WorldGridTile>());
		
        if(!m_ownedTiles.ContainsKey(e.m_player.PlayerId))
            m_ownedTiles.Add(e.m_player.PlayerId, new List<WorldGridTile>());
    }
	
    private void OnPlayerRemoved(object sender, PlayerRemovedEventArgs e)
    {
        if (m_trailTiles.ContainsKey(e.m_player.PlayerId))
        {
            if (NetworkServer.active && !e.m_player.isOwned)
            {
                List<WorldGridTile> trailTiles = new List<WorldGridTile>(m_trailTiles[e.m_player.PlayerId]);
				
                foreach (WorldGridTile trailTile in trailTiles)
                {
                    TileStatus oldStatus = trailTile.TileStatus;
					TileStatus newStatus = new TileStatus() {PendingOwnerPlayerId = null, OwnerPlayerId = oldStatus.OwnerPlayerId};
                    
                    m_grid.SetTileStatus(trailTile.m_gridPos, newStatus);
                }		
            }
			
            m_trailTiles.Remove(e.m_player.PlayerId);	
        }

        if (m_ownedTiles.ContainsKey(e.m_player.PlayerId))
        {
            if (NetworkServer.active && !e.m_player.isOwned)
            {
                List<WorldGridTile> ownedTiles = new List<WorldGridTile>(m_ownedTiles[e.m_player.PlayerId]);

                foreach (WorldGridTile ownedTile in ownedTiles)
                {
                    TileStatus oldStatus = ownedTile.TileStatus;
                    TileStatus newStatus = new TileStatus() {PendingOwnerPlayerId = oldStatus.PendingOwnerPlayerId, OwnerPlayerId = null};
                    m_grid.SetTileStatus(ownedTile.m_gridPos, newStatus);
                }
            }
			
            m_ownedTiles.Remove(e.m_player.PlayerId);	
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
        
        //Remove old trail tile association
        if (!string.IsNullOrEmpty(oldStatus.PendingOwnerPlayerId) && oldStatus.PendingOwnerPlayerId != newStatus.PendingOwnerPlayerId)
        {
            if(m_trailTiles[oldStatus.PendingOwnerPlayerId].Contains(tile))
                m_trailTiles[oldStatus.PendingOwnerPlayerId].Remove(tile);   
        }

        //Add new trail tile association
        if (!string.IsNullOrEmpty(newStatus.PendingOwnerPlayerId) && oldStatus.PendingOwnerPlayerId != newStatus.PendingOwnerPlayerId)
        {
            if(!m_trailTiles[newStatus.PendingOwnerPlayerId].Contains(tile))
                m_trailTiles[newStatus.PendingOwnerPlayerId].Add(tile);   
        }

        //Remove old owned tile association
        if (!string.IsNullOrEmpty(oldStatus.OwnerPlayerId) && oldStatus.OwnerPlayerId != newStatus.OwnerPlayerId)
        {
            if(m_ownedTiles[oldStatus.OwnerPlayerId].Contains(tile))
                m_ownedTiles[oldStatus.OwnerPlayerId].Remove(tile);   
        }

        //Add new owned tile association
        if (!string.IsNullOrEmpty(newStatus.OwnerPlayerId))
        {
            if(!m_ownedTiles[newStatus.OwnerPlayerId].Contains(tile))
                m_ownedTiles[newStatus.OwnerPlayerId].Add(tile);   
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
            if (m_trailTiles[playerId].Count > 0)
            {
                Player player = GameClient.Instance.GameWorld.GetPlayerFromId(playerId);

                EncloseLoop(data.m_player.PlayerId, tile,player.m_lastOwnedTileSteppedOn, m_trailTiles[playerId], out List<WorldGridTile> loop);
                FillEnclosedTiles(data.m_player.PlayerId, loop);
            }
        }
    }

    private void FillEnclosedTiles(string playerId, List<WorldGridTile> loop)
    {
        //If loop is the same as trail, we failed to enclose a loop. Just fill trail tiles
        if (loop.Count == m_trailTiles[playerId].Count)
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

         Pathfinding pathfinding = GameClient.Instance.GameWorld.Pathfinding;
         List<PathNode> path = pathfinding.FindPath(m_ownedTiles[playerId], startTile, endTile);

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
        if (!m_ownedTiles.ContainsKey(playerId))
            return null;

        return m_ownedTiles[playerId];
    }

    public List<WorldGridTile> GetTrailTiles(string playerId)
    {
        if (!m_trailTiles.ContainsKey(playerId))
            return null;

        return m_trailTiles[playerId];
    }

}