using System;
using System.Collections;
using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class WorldGridNode : GridNode
{
    public string m_ownerPlayerId = null;
    public string m_trailedByPlayerId = null;

    public WorldGridTile m_tile;

    public void SetOwner(Player player)
    {
        m_ownerPlayerId = player.PlayerId;
        
        player.m_ownedNodes.Add(this);
        
        m_tile.UpdateMaterial(player.PlayerData.TerritoryMaterial);
    }

    public void SetTrail(Player player)
    {
        m_trailedByPlayerId = player.PlayerId;
        
        m_tile.UpdateMaterial(player.PlayerData.TrailMaterial);
    }
}
public class WorldGrid : Singleton<WorldGrid>
{
    [SerializeField] 
    private bool m_debug = true;

    [SerializeField] 
    private Color m_debugNodeColor = Color.cyan;

    [SerializeField] 
    private WorldGridTile m_tilePrefab;
    
    [SerializeField]
    private Vector2 m_worldSize = new Vector2(10, 10);

    [SerializeField] 
    private float m_nodeRadius = 1;
    
    private Grid<WorldGridNode> m_grid;
    public Grid<WorldGridNode> Grid => m_grid;

    private void OnValidate()
    {
        m_grid = new Grid<WorldGridNode>(transform.position, m_worldSize, m_nodeRadius);
    }

    private void Awake()
    {
        m_grid = new Grid<WorldGridNode>(transform.position, m_worldSize, m_nodeRadius, true, OnNodeCreated);
    }

    private void OnNodeCreated(WorldGridNode node)
    {
        if(m_tilePrefab == null)
            return;

        WorldGridTile tileInstance = Instantiate(m_tilePrefab, node.m_worldPosition, Quaternion.identity);
        
        float nodeDiameter = m_nodeRadius * 2;
        float bias = 0.02f;
        
        tileInstance.transform.localScale = new Vector3(nodeDiameter - bias, tileInstance.transform.localScale.y, nodeDiameter - bias);
        tileInstance.transform.SetParent(transform);
        
        node.m_tile = tileInstance;
    }
    
    private void OnDrawGizmos()
    {
        if(!m_debug)
            return;
        
        if(m_grid == null)
            return;

        Gizmos.color = m_debugNodeColor;

        float bias = 0.02f;

        float nodeDiameter = m_nodeRadius * 2;
        
        for (int x = 0; x < m_grid.GridSize.x; x++)
        {
            for (int y = 0; y < m_grid.GridSize.y; y++)
            {
                WorldGridNode node = m_grid.Nodes[x, y];
                
                Gizmos.DrawCube(node.m_worldPosition, new Vector3(nodeDiameter - bias, 0.1f, nodeDiameter - bias));
            }
        }
        
        Gizmos.color = Color.white;
        
    }
}
