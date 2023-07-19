using System;
using System.Collections;
using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayArea : MonoBehaviour
{
    [SerializeField] 
    private bool m_debug = true;

    [SerializeField] 
    private Color m_debugTileColor = Color.cyan;

	[SerializeField] 
    private WorldGridTile m_tilePrefab;
    
    [SerializeField]
    private Vector2 m_worldSize = new Vector2(10, 10);

    [SerializeField] 
    private float m_tileHalfSize = 1;
    public float TileHalfSize => m_tileHalfSize;

    public static PlayArea Instance { get; private set; }

    public WorldGridTile[,] Tiles => m_tiles;
    private WorldGridTile[,] m_tiles;
    
    public Vector2Int GridSize => m_gridSize;
    private Vector2Int m_gridSize;

    public float TileSize => m_tileHalfSize * 2;
    
    
    private void Awake()
    {
        Instance = this;
        
        CreateGrid();
    }

    private void SetupVariables()
    {
	    m_gridSize.x = Mathf.RoundToInt(m_worldSize.x/ TileSize);
	    m_gridSize.y = Mathf.RoundToInt(m_worldSize.y/ TileSize);
    }
	/// <summary>
	/// Creates grid
	/// </summary>
	public void CreateGrid()
	{
		SetupVariables();
		
		m_tiles = new WorldGridTile[m_gridSize.x, m_gridSize.y];

		float bias = 0.02f;
		
		for (int x = 0; x < m_gridSize.x; x ++) 
		{
			for (int y = 0; y < m_gridSize.y; y ++) 
			{
				Vector3 worldPoint = GetWorldPointFromGridPoint(x, y);
				Vector2Int gridPos = new Vector2Int(x, y);

				WorldGridTile newTile = Instantiate(m_tilePrefab, worldPoint, Quaternion.identity);
				newTile.transform.localScale = new Vector3(TileSize - bias, newTile.transform.localScale.y, TileSize - bias);
				newTile.m_gridPos = gridPos;
				
				newTile.transform.SetParent(transform);
				
				m_tiles[x, y] = newTile;
			}
		}
	}
	
	/// <summary>
	/// Gets tile by world position
	/// </summary>
	/// <param name="worldPosition">World position</param>
	/// <returns>Returns tile closest to world position</returns>
	public WorldGridTile GetTile(Vector3 worldPosition)
	{
		WorldGridTile closestTile = null;
		float closestDist = Mathf.Infinity;
		
		for (int x = 0; x < m_gridSize.x; x++)
		{
			for (int y = 0; y < m_gridSize.y; y++)
			{
				WorldGridTile tile = m_tiles[x, y];

				float sqrDistToTile = (worldPosition - m_tiles[x, y].transform.position).sqrMagnitude;

				if (sqrDistToTile < closestDist)
				{
					closestDist = sqrDistToTile;
					closestTile = tile;
				}
			}
		}

		return closestTile;
	}
	
	/// <summary>
	/// Gets tile by grid position
	/// </summary>
	/// <param name="x">Grid X position</param>
	/// <param name="y">Grid Y position</param>
	/// <returns>Returns tile in grid position</returns>
	public WorldGridTile GetTile(int x, int y)
	{
		if (x >= m_gridSize.x || y >= m_gridSize.y || x < 0 || y < 0)
			return null;

		return m_tiles[x, y];
	}
	
	/// <summary>
	/// Gets neighbour in a direction from tile
	/// </summary>
	/// <param name="tile">Tile to get neighbour of</param>
	/// <param name="direction">Direction</param>
	/// <returns>Returns neighbour in direction</returns>
	public WorldGridTile GetNeighbour(WorldGridTile tile, Vector2Int direction)
	{
		direction.x = Mathf.Clamp(direction.x, -1, 1);
		direction.y = Mathf.Clamp(direction.y, -1, 1);

		if (tile == null || direction == Vector2Int.zero)
			return null;

		return GetTile(tile.m_gridPos.x + direction.x, tile.m_gridPos.y + direction.y);
	}
	
	/// <summary>
	/// Get neighbouring tiles
	/// </summary>
	/// <param name="tile">Tile to get neighbours of</param>
	/// <returns>Returns neighbouring tiles</returns>
	public List<WorldGridTile> GetNeighbours(WorldGridTile tile)
	{
		List<WorldGridTile> neighbours = new List<WorldGridTile>();
    
		if (tile.m_gridPos.x > 0)
		{
			//Left
			neighbours.Add(GetNeighbour(tile, new Vector2Int(-1, 0)));
        
			//Left Down
			if(tile.m_gridPos.y > 0)
				neighbours.Add(GetNeighbour(tile, new Vector2Int(-1, -1)));
        
			//Left Up
			if(tile.m_gridPos.y < m_gridSize.y - 1)
				neighbours.Add(GetNeighbour(tile, new Vector2Int(-1, 1)));
		}

		if (tile.m_gridPos.x < m_gridSize.x - 1)
		{
			//Right
			neighbours.Add(GetNeighbour(tile, new Vector2Int(1, 0)));
        
			//Right Down
			if(tile.m_gridPos.y > 0)
				neighbours.Add(GetNeighbour(tile, new Vector2Int(1, -1)));
        
			//Right Up
			if(tile.m_gridPos.y < m_gridSize.y - 1)
				neighbours.Add(GetNeighbour(tile, new Vector2Int(1, 1)));
		}
    
		//Down
		if(tile.m_gridPos.y > 0)
			neighbours.Add(GetNeighbour(tile, new Vector2Int(0, -1)));
    
		//Up
		if(tile.m_gridPos.y < m_gridSize.y - 1)
			neighbours.Add(GetNeighbour(tile, new Vector2Int(0, 1)));
    
		return neighbours;
	}
		
	private Vector3 GetWorldPointFromGridPoint(int x, int y)
	{
		return transform.position + Vector3.right * (x * TileSize + m_tileHalfSize) + Vector3.forward * (y * TileSize + m_tileHalfSize);
	}
	
    private void OnDrawGizmos()
    {
        if(!m_debug)
            return;
        
        SetupVariables();

        Gizmos.color = m_debugTileColor;
        
        float bias = 0.02f;

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
	            Vector3 worldPoint = GetWorldPointFromGridPoint(x, y);
                
                Gizmos.DrawCube(worldPoint, new Vector3(TileSize - bias, 0.1f, TileSize - bias));
            }
        }
        
        Gizmos.color = Color.white;
        
    }
}
