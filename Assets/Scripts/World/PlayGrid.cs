using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;

public class PlayGrid : NetworkedWorldGrid<WorldGridTile>
{
	public static PlayGrid Instance { get; private set; }
	
    protected override void Awake()
    {
        Instance = this;
        
        base.Awake();
    }
    
    protected override void OnNodeCreated(WorldGridTile node)
    {
	    base.OnNodeCreated(node);
	    
	    float bias = 0.02f;

	    Transform nodeTransform = node.transform;
	    nodeTransform.localScale = new Vector3(NodeDiameter - bias, nodeTransform.localScale.y, NodeDiameter - bias);
	    nodeTransform.SetParent(transform);
    }

    [ClientRpc]
    public void SetTilePendingOwner(Vector2Int nodePos, string playerId)
    {
	    WorldGridTile tile = GetNode(nodePos.x, nodePos.y);

	    if (tile == null)
	    {
		    Debug.LogError($"Failed to set tile pending owner to player {playerId}. No tile at position {nodePos}", gameObject);
		    return;
	    }

	    Player player = GameClient.Instance.GameWorld.GetPlayerFromId(playerId);

	    if (player == null)
	    {
		    Debug.LogError($"Failed to set tile pending owner to player {playerId}. No player associated with id {playerId}");
		    return;
	    }
	    
	    tile.SetPendingOwner(player);
    }

    [ClientRpc]
    public void SetTileOwner(Vector2Int nodePos, string playerId)
    {
	    WorldGridTile tile = GetNode(nodePos.x, nodePos.y);

	    if (tile == null)
	    {
		    Debug.LogError($"Failed to set tile owner to player {playerId}. No tile at position {nodePos}", gameObject);
		    return;
	    }

	    Player player = GameClient.Instance.GameWorld.GetPlayerFromId(playerId);

	    if (player == null)
	    {
		    Debug.LogError($"Failed to set tile owner to player {playerId}. No player associated with id {playerId}");
		    return;
	    }
	    
	    tile.SetOwner(player);
    }
}
