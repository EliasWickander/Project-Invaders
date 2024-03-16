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

	private Dictionary<Vector2Int, TileStatus> m_networkedTiles = new Dictionary<Vector2Int, TileStatus>();

	protected override void Awake()
    {
        Instance = this;
	    
        m_networkedTiles.Clear();
        
        base.Awake();
    }

    protected override void OnTileCreated(WorldGridTile tile)
    {
	    base.OnTileCreated(tile);

	    float bias = 0.02f;

	    Transform tileTransform = tile.transform;
	    tileTransform.localScale = new Vector3(NodeDiameter - bias, tileTransform.localScale.y, NodeDiameter - bias);
	    tileTransform.SetParent(transform);

	    tile.OnStatusChanged += OnTileStatusChanged;

	    m_networkedTiles.Add(tile.m_gridPos, new TileStatus());
    }

    private void OnTileStatusChanged(object sender, TileStatusChangedEventArgs e)
    {
	    if (sender is WorldGridTile tile)
	    {
		    TileStatus oldStatus = e.m_oldStatus;
		    TileStatus newStatus = e.m_newStatus;

		    if (NetworkClient.active)
		    {
			    if(m_onTileStatusChangedClientEvent != null)
				    m_onTileStatusChangedClientEvent.Raise(new OnTileStatusChangedEventData() {m_tile = tile, m_oldStatus = oldStatus, m_newStatus = newStatus});
		    }

		    if (NetworkServer.active)
		    {
			    if(m_onTileStatusChangedServerEvent != null)
				    m_onTileStatusChangedServerEvent.Raise(new OnTileStatusChangedEventData() {m_tile = tile, m_oldStatus = oldStatus, m_newStatus = newStatus});
		    }
	    }
    }

    public void SetTileStatus(Vector2Int tilePos, TileStatus status)
    {
	    if (!m_networkedTiles.Keys.Contains(tilePos))
	    {
		    Debug.LogError($"Attempted to set tile status of tile at position {tilePos} but no tile associated with position");
		    return;
	    }

	    m_networkedTiles[tilePos] = status;

	    WorldGridTile tile = GetNode(tilePos.x, tilePos.y);
	    
	    //Set tile local status
	    tile.SetStatus(status);
    }

    private bool GetTileStatus(Vector2Int tilePos, out TileStatus outTileStatus)
    {
	    outTileStatus = new TileStatus();

	    if (!m_networkedTiles.Keys.Contains(tilePos))
	    {
		    Debug.LogError($"Attempted to get tile status of tile at position {tilePos} but no tile associated with position");
		    return false;
	    }

	    outTileStatus = m_networkedTiles[tilePos];
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
