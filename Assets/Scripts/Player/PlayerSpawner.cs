using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] 
    private int m_startTerritoryRadius = 2;
    
    private PlayGrid m_playGrid;
    
    public void OnGameStart()
    {
        m_playGrid = PlayGrid.Instance;
        
        foreach (Player player in NetworkManagerCustom.Instance.GamePlayers)
        {
            player.OnSpawnedEventServer += OnPlayerSpawnedServer;
            player.OnDeathEventServer += OnPlayerDeath;
        }
    }
    
    private void OnDestroy()
    {
        if(NetworkManagerCustom.Instance == null)
            return;
        
        foreach (Player player in NetworkManagerCustom.Instance.GamePlayers)
        {
            player.OnSpawnedEventServer -= OnPlayerSpawnedServer;
            player.OnDeathEventServer -= OnPlayerDeath;
        }
    }

    [Server]
    private void SpawnPlayer(Player player)
    {
        if(player == null)
            return;

        Transform spawnPoint = NetworkManagerCustom.Instance.GetStartPosition();
        
        player.transform.position = spawnPoint.position;
        
        player.OnSpawned(spawnPoint);
    }

    private void OnPlayerSpawnedServer(Player player)
    {
        NetworkManagerCustom.Instance.DisableStartPoint(player.SpawnTransform);
        
        WorldGridTile spawnTile = m_playGrid.GetNode(player.SpawnTransform.position);
        
        //SetOwnerArea(player, spawnNode);
    }

    private void OnPlayerDeath(Player player)
    {
        NetworkManagerCustom.Instance.EnableStartPoint(player.SpawnTransform);
        
        SpawnPlayer(player);
    }
    
    private void SetOwnerArea(Player player, WorldGridTile sourceTile)
    {
        List<WorldGridTile> nodesWithinRadius = GetNodesWithinRadius(sourceTile, m_startTerritoryRadius);

        foreach (WorldGridTile node in nodesWithinRadius)
        {
            node.SetOwner(player);
        }
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
                foreach (WorldGridTile neighborNode in m_playGrid.GetNeighbours(currentNode))
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
}
