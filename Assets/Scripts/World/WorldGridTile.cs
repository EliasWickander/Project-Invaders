using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGridTile : MonoBehaviour
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;
    
    public Vector2Int m_gridPos;

    public string m_ownerPlayerId = null;
    public string m_trailedByPlayerId = null;

    public void UpdateMaterial(Material material)
    {
        m_meshRenderer.material = material;
    }

    public void SetOwner(Player player)
    {
        UpdateMaterial(player.PlayerData.TerritoryMaterial);
    }

    public void SetTrail(Player player)
    {
        UpdateMaterial(player.PlayerData.TrailMaterial);
    }
}
