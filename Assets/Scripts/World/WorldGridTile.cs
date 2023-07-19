using System.Collections;
using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldGridTile : WorldGridNode
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;

    public string OwnerPlayerId = null;
    public string PendingOwnerPlayerId = null;

    public void UpdateMaterial(Material material)
    {
        m_meshRenderer.material = material;
    }

    public void SetOwner(Player player)
    {
        OwnerPlayerId = player.PlayerId;
        
        UpdateMaterial(player.PlayerData.TerritoryMaterial);
    }
    
    public void SetTrail(Player player)
    {
        PendingOwnerPlayerId = player.PlayerId;
        
        UpdateMaterial(player.PlayerData.TrailMaterial);
    }
}
