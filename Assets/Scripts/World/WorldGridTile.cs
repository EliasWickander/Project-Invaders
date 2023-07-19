using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldGridTile : MonoBehaviour
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;
    
    public Vector2Int m_gridPos;

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
