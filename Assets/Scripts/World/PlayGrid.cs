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
	[SerializeField] 
	private Server_OnTileStatusChangedEvent m_onTileStatusChangedServerEvent;
    
	[SerializeField] 
	private Client_OnTileStatusChangedEvent m_onTileStatusChangedClientEvent;
	
	public static PlayGrid Instance { get; private set; }

	private SyncDictionary<Vector2Int, TileStatus> m_tileNetworkedStatus = new SyncDictionary<Vector2Int, TileStatus>();
	public SyncDictionary<Vector2Int, TileStatus> TileNetworkedStatus => m_tileNetworkedStatus;

	protected override void Awake()
    {
        Instance = this;
        
        base.Awake();
    }

	public override void OnStartClient()
    {
	    m_tileNetworkedStatus.Callback += OnNetworkTileStatusChanged;

	    foreach (KeyValuePair<Vector2Int, TileStatus> kvp in m_tileNetworkedStatus)
	    {
		    //Sync down current tile status from server to client on startup
		    OnNetworkTileStatusChanged(SyncIDictionary<Vector2Int, TileStatus>.Operation.OP_ADD, kvp.Key, default, kvp.Value);
	    }
    }

    public override void OnStopClient()
    {
	    m_tileNetworkedStatus.Callback -= OnNetworkTileStatusChanged;
    }

    private void OnNetworkTileStatusChanged(SyncIDictionary<Vector2Int, TileStatus>.Operation op, Vector2Int key, TileStatus oldStatus, TileStatus newStatus)
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

		    if (!NetworkServer.active)
		    {
			    if(m_onTileStatusChangedClientEvent != null)
				    m_onTileStatusChangedClientEvent.Raise(new OnTileStatusChangedEventData() {m_tile = tile, m_oldStatus = oldStatus, m_newStatus = newStatus});
		    }
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

	    TileStatus oldStatus = m_tileNetworkedStatus[tilePos];
	    m_tileNetworkedStatus[tilePos] = status;
	    
	    if(m_onTileStatusChangedServerEvent != null)
		    m_onTileStatusChangedServerEvent.Raise(new OnTileStatusChangedEventData() {m_tile = GetNode(tilePos.x, tilePos.y), m_oldStatus = oldStatus, m_newStatus = m_tileNetworkedStatus[tilePos]});
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
