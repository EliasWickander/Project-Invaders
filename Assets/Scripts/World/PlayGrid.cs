using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;

public struct NetworkedTile
{
	public Vector2Int GridPos;
	public string OwnerPlayerId;
	public string PendingOwnerPlayerId;
}

public struct TileStatus
{
	public string OwnerPlayerId;
	public string PendingOwnerPlayerId;
}

public class PathNode
{
	public WorldGridTile m_tile;
	public PathNode Parent { get; set; }
	public float GCost { get; set; }
	public float HCost { get; set; }
	public float FCost => GCost + HCost;

	public PathNode(WorldGridTile tile)
	{
		m_tile = tile;
		GCost = 0;
		HCost = 0;
		Parent = null;
	}
}

public class Pathfinding
{
	public static List<PathNode> FindPath(Player player, WorldGridTile startNode, WorldGridTile endNode)
	{
		List<WorldGridTile> ownedTiles = PlayGrid.Instance.GetOwnedTiles(player.PlayerId);
		
		Dictionary<WorldGridTile, PathNode> pathNodes = new Dictionary<WorldGridTile, PathNode>();

		foreach (WorldGridTile ownedTile in ownedTiles)
		{
			pathNodes.Add(ownedTile, new PathNode(ownedTile));	
		}
		
		List<PathNode> openSet = new List<PathNode>();
		List<PathNode> closedSet = new List<PathNode>();

		PathNode startPfNode = new PathNode(startNode);
		PathNode endPfNode = new PathNode(endNode);
		
		openSet.Add(startPfNode);

		PathNode currentNode = startPfNode;

		bool pathFound = false;
		while (pathFound == false)
		{
			float minFCost = openSet.Min(x => x.FCost);
			currentNode = openSet.First(x => x.FCost == minFCost);

			openSet.Remove(currentNode);
			closedSet.Add(currentNode);

			foreach (WorldGridTile neighbour in PlayGrid.Instance.GetNeighbours(currentNode.m_tile, false))
			{
				if (neighbour == endPfNode.m_tile)
				{
					pathFound = true;
					break;
				}
				
				if(!pathNodes.ContainsKey(neighbour))
					continue;

				PathNode neighbourPfNode = pathNodes[neighbour];

				if(closedSet.Contains(neighbourPfNode))
					continue;

				float tentativeGCost = currentNode.GCost + Distance(currentNode, neighbourPfNode);

				if (tentativeGCost < neighbourPfNode.GCost || !openSet.Contains(neighbourPfNode))
				{
					neighbourPfNode.GCost = tentativeGCost;
					neighbourPfNode.HCost = Heuristic(neighbourPfNode, endPfNode);
					neighbourPfNode.Parent = currentNode;
					
					if(!openSet.Contains(neighbourPfNode))
						openSet.Add(neighbourPfNode);
				}
			}

			if (openSet.Count == 0)
			{
				Debug.Log("No path found");
				break;
			}
		}

		if (pathFound)
		{
			List<PathNode> path = new List<PathNode>();
			endPfNode.Parent = currentNode;
			path.Add(endPfNode);
			while (currentNode != null)
			{
				path.Add(currentNode);
				currentNode = currentNode.Parent;
			}

			path.Reverse();
			return path;
		}

		return null;
	}

	public static float Distance(PathNode first, PathNode second)
	{
		return Mathf.Sqrt(Mathf.Pow(first.m_tile.m_gridPos.x - second.m_tile.m_gridPos.x, 2) + Mathf.Pow(first.m_tile.m_gridPos.y - second.m_tile.m_gridPos.y, 2));
	}

	public static float Heuristic(PathNode first, PathNode second)
	{
		return Mathf.Abs(first.m_tile.m_gridPos.x - second.m_tile.m_gridPos.x) + Mathf.Abs(first.m_tile.m_gridPos.y - second.m_tile.m_gridPos.y);
	}
}
public class PlayGrid : NetworkedWorldGrid<WorldGridTile>
{
	public static PlayGrid Instance { get; private set; }

	public SyncDictionary<Vector2Int, TileStatus> m_tileNetworkedStatus = new SyncDictionary<Vector2Int, TileStatus>();

	private Dictionary<string, List<WorldGridTile>> m_ownedTiles = new Dictionary<string, List<WorldGridTile>>();
	private Dictionary<string, List<WorldGridTile>> m_trailTiles = new Dictionary<string, List<WorldGridTile>>();
	
	protected override void Awake()
    {
        Instance = this;
        
        base.Awake();
    }

	private void Start()
	{
		GameWorld.OnPlayerAddedEvent += OnPlayerAdded;
		GameWorld.OnPlayerRemovedEvent += OnPlayerRemoved;
	}

	private void OnDestroy()
	{
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
					TileStatus oldStatus = m_tileNetworkedStatus[trailTile.m_gridPos];
					
					m_tileNetworkedStatus[trailTile.m_gridPos] = new TileStatus() {PendingOwnerPlayerId = null, OwnerPlayerId = oldStatus.OwnerPlayerId};
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
					TileStatus oldStatus = m_tileNetworkedStatus[ownedTile.m_gridPos];
					
					m_tileNetworkedStatus[ownedTile.m_gridPos] = new TileStatus() {PendingOwnerPlayerId = oldStatus.PendingOwnerPlayerId, OwnerPlayerId = null};
				}
			}
			
			m_ownedTiles.Remove(e.m_player.PlayerId);	
		}
	}

	public override void OnStartClient()
    {
	    m_tileNetworkedStatus.Callback += OnTileStatusChanged;

	    foreach (KeyValuePair<Vector2Int, TileStatus> kvp in m_tileNetworkedStatus)
	    {
		    //Sync down current tile status from server to client on startup
		    OnTileStatusChanged(SyncIDictionary<Vector2Int, TileStatus>.Operation.OP_ADD, kvp.Key, default, kvp.Value);
	    }
    }

    public override void OnStopClient()
    {
	    m_tileNetworkedStatus.Callback -= OnTileStatusChanged;
    }

    private void OnTileStatusChanged(SyncIDictionary<Vector2Int, TileStatus>.Operation op, Vector2Int key, TileStatus oldStatus, TileStatus newStatus)
    {
	    WorldGridTile tile = GetNode(key.x, key.y);

	    if (tile == null)
	    {
		    Debug.LogError($"Failed to set local tile status of tile at position {key}, but no tile is associated with position", gameObject);
		    return;
	    }
	    
	    //Set tile local status
	    tile.SetStatus(newStatus);
    }
    
    protected override void OnTileCreated(WorldGridTile tile)
    {
	    base.OnTileCreated(tile);
	    
	    float bias = 0.02f;

	    Transform tileTransform = tile.transform;
	    tileTransform.localScale = new Vector3(NodeDiameter - bias, tileTransform.localScale.y, NodeDiameter - bias);
	    tileTransform.SetParent(transform);

	    tile.OnStatusChanged += OnTileStatusChanged;
	    
	    if (NetworkServer.active)
	    {
		    m_tileNetworkedStatus.Add(tile.m_gridPos, new TileStatus());
	    }
    }

    private void OnTileStatusChanged(object sender, TileStatusChangedEventArgs e)
    {
	    if (sender is WorldGridTile tile)
	    {
		    TileStatus oldStatus = e.m_oldStatus;
		    TileStatus newStatus = e.m_newStatus;
		    
		    //Remove old trail tile association
		    if (!string.IsNullOrEmpty(oldStatus.PendingOwnerPlayerId) && oldStatus.PendingOwnerPlayerId != newStatus.PendingOwnerPlayerId)
			    m_trailTiles[oldStatus.PendingOwnerPlayerId].Remove(tile);

		    //Add new trail tile association
		    if (!string.IsNullOrEmpty(newStatus.PendingOwnerPlayerId))
		    {
			    if(!m_trailTiles[newStatus.PendingOwnerPlayerId].Contains(tile))
					m_trailTiles[newStatus.PendingOwnerPlayerId].Add(tile);   
		    }

		    //Remove old owned tile association
		    if (!string.IsNullOrEmpty(oldStatus.OwnerPlayerId) && oldStatus.OwnerPlayerId != newStatus.OwnerPlayerId)
			    m_ownedTiles[oldStatus.OwnerPlayerId].Remove(tile);

		    //Add new owned tile association
		    if (!string.IsNullOrEmpty(newStatus.OwnerPlayerId))
		    {
			    if(!m_ownedTiles[newStatus.OwnerPlayerId].Contains(tile))
					m_ownedTiles[newStatus.OwnerPlayerId].Add(tile);   
		    }
	    }
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

    [Server]
    public void SetTileStatus(Vector2Int tilePos, TileStatus status)
    {
	    if (!m_tileNetworkedStatus.Keys.Contains(tilePos))
	    {
		    Debug.LogError($"Attempted to set tile status of tile at position {tilePos} but no tile associated with position");
		    return;
	    }

	    m_tileNetworkedStatus[tilePos] = status;
    }

    private bool GetTileStatus(Vector2Int tilePos, out TileStatus outTileStatus)
    {
	    outTileStatus = new TileStatus();
	    
	    if (!m_tileNetworkedStatus.Keys.Contains(tilePos))
	    {
		    Debug.LogError($"Attempted to get tile status of tile at position {tilePos} but no tile associated with position");
		    return false;
	    }
	    
	    outTileStatus = m_tileNetworkedStatus[tilePos];
	    return true;
    }
    public void SetTileOwner(Vector2Int tilePos, string playerId)
    {
	    if (GetTileStatus(tilePos, out TileStatus oldTileStatus))
	    {
		    //If player went from pending owner to owner, remove pending status
		    string pendingOwner = oldTileStatus.PendingOwnerPlayerId == playerId ? null : oldTileStatus.PendingOwnerPlayerId;
		    
		    SetTileStatus(tilePos, new TileStatus(){PendingOwnerPlayerId = pendingOwner, OwnerPlayerId = playerId});   
	    }
    }

    public void SetTilePendingOwner(Vector2Int tilePos, string playerId)
    {
	    if (GetTileStatus(tilePos, out TileStatus oldTileStatus))
	    {
		    SetTileStatus(tilePos, new TileStatus() {PendingOwnerPlayerId = playerId, OwnerPlayerId = oldTileStatus.OwnerPlayerId });
	    }
    }
}
