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
    
    private WorldGrid m_worldGrid;
    
    public void OnGameStart()
    {
        m_worldGrid = WorldGrid.Instance;
        
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
        
        WorldGridNode spawnNode = m_worldGrid.Grid.GetNode(player.SpawnTransform.position);
        
        //SetOwnerArea(player, spawnNode);
    }

    private void OnPlayerDeath(Player player)
    {
        NetworkManagerCustom.Instance.EnableStartPoint(player.SpawnTransform);
        
        SpawnPlayer(player);
    }
    
    private void SetOwnerArea(Player player, WorldGridNode sourceNode)
    {
        List<WorldGridNode> nodesWithinRadius = GetNodesWithinRadius(sourceNode, m_startTerritoryRadius);

        foreach (WorldGridNode node in nodesWithinRadius)
        {
            node.SetOwner(player);
        }
    }
    
    public List<WorldGridNode> GetNodesWithinRadius(WorldGridNode sourceNode, int radius)
    {
        List<WorldGridNode> nodesWithinRadius = new List<WorldGridNode>();
        Queue<(WorldGridNode node, int distance)> queue = new Queue<(WorldGridNode, int)>();
        HashSet<WorldGridNode> visited = new HashSet<WorldGridNode>();

        queue.Enqueue((sourceNode, 0));
        visited.Add(sourceNode);

        while (queue.Count > 0)
        {
            (WorldGridNode currentNode, int distance) = queue.Dequeue();
            nodesWithinRadius.Add(currentNode);

            if (distance < radius)
            {
                foreach (WorldGridNode neighborNode in m_worldGrid.Grid.GetNeighbours(currentNode))
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
