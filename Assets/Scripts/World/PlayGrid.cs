using System.Collections.Generic;
using System.Linq;
using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;

public struct TileStatus
{
	public string OwnerPlayerId;
	public string PendingOwnerPlayerId;
}

public class PlayGrid : NetworkedWorldGrid<WorldGridTile>
{
	public static PlayGrid Instance { get; private set; }

	public SyncDictionary<Vector2Int, TileStatus> m_tileNetworkedStatus = new SyncDictionary<Vector2Int, TileStatus>();
	
    protected override void Awake()
    {
        Instance = this;
        
        base.Awake();
    }

    public override void OnStartClient()
    {
	    m_tileNetworkedStatus.Callback += OnTileStatusChanged;

	    foreach (KeyValuePair<Vector2Int, TileStatus> kvp in m_tileNetworkedStatus)
	    {
		    //Sync down current tile status from server to client on startup
		    OnTileStatusChanged(SyncIDictionary<Vector2Int, TileStatus>.Operation.OP_ADD, kvp.Key, kvp.Value);
	    }
    }

    public override void OnStopClient()
    {
	    m_tileNetworkedStatus.Callback -= OnTileStatusChanged;
    }

    private void OnTileStatusChanged(SyncIDictionary<Vector2Int, TileStatus>.Operation op, Vector2Int key, TileStatus item)
    {
	    WorldGridTile tile = GetNode(key.x, key.y);

	    if (tile == null)
	    {
		    Debug.LogError($"Failed to set local tile status of tile at position {key}, but no tile is associated with position", gameObject);
		    return;
	    }
	    
	    //Set tile local status
	    tile.SetStatus(item);
    }
    
    protected override void OnNodeCreated(WorldGridTile node)
    {
	    base.OnNodeCreated(node);
	    
	    float bias = 0.02f;

	    Transform nodeTransform = node.transform;
	    nodeTransform.localScale = new Vector3(NodeDiameter - bias, nodeTransform.localScale.y, NodeDiameter - bias);
	    nodeTransform.SetParent(transform);

	    if (NetworkServer.active)
	    {
		    m_tileNetworkedStatus.Add(node.m_gridPos, new TileStatus());
	    }
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

    public void SetTileOwner(Vector2Int tilePos, string playerId)
    {
	    SetTileStatus(tilePos, new TileStatus(){OwnerPlayerId = playerId});
    }

    public void SetTilePendingOwner(Vector2Int tilePos, string playerId)
    {
	    SetTileStatus(tilePos, new TileStatus(){PendingOwnerPlayerId = playerId});
    }
}
