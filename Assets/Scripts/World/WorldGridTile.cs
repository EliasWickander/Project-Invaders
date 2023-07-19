using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldGridTile : NetworkBehaviour
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;
    
    public Vector2Int m_gridPos;

    [SyncVar(hook = nameof(OnOwnerChanged))]
    public string OwnerPlayerId = null;
    
    [SyncVar(hook = nameof(OnPendingOwnerChanged))]
    public string PendingOwnerPlayerId = null;

    public void UpdateMaterial(Material material)
    {
        m_meshRenderer.material = material;
    }

    [Server]
    public void SetOwner(Player player)
    {
        OwnerPlayerId = player.PlayerId;
    }

    [Server]
    public void SetTrail(Player player)
    {
        PendingOwnerPlayerId = player.PlayerId;
    }

    private void OnOwnerChanged(string oldOwner, string newOwner)
    {
        GameWorld world = GameClient.Instance.GameWorld;
        Player player = world.GetPlayerFromId(newOwner);
        
        UpdateMaterial(player.PlayerData.TerritoryMaterial);
    }
    
    private void OnPendingOwnerChanged(string oldOwner, string newOwner)
    {
        GameWorld world = GameClient.Instance.GameWorld;
        Player player = world.GetPlayerFromId(newOwner);
        
        UpdateMaterial(player.PlayerData.TrailMaterial);
    }
}
