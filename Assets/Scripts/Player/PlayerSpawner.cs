using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] 
    private Player m_player;

    [SerializeField] 
    private int m_startTerritoryRadius = 2;
    
    private WorldGrid m_worldGrid;

    private void Start()
    {
        m_worldGrid = WorldGrid.Instance;
        
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if(m_player == null)
            return;

        int randX = Random.Range(m_startTerritoryRadius, m_worldGrid.Grid.GridSize.x - m_startTerritoryRadius);
        int randY = Random.Range(m_startTerritoryRadius, m_worldGrid.Grid.GridSize.y - m_startTerritoryRadius);
        
        WorldGridNode spawnNode = m_worldGrid.Grid.GetNode(randX, randY);

        Player playerInstance = Instantiate(m_player, spawnNode.m_worldPosition, Quaternion.identity);
        
        SetOwnerArea(playerInstance, spawnNode);
        
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
