using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class WorldGridTile : WorldGridNode
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;

    private TileStatus m_tileStatus = new TileStatus();
    public TileStatus TileStatus => m_tileStatus;

    private Material m_defaultMaterial;

    private void Awake()
    {
        m_defaultMaterial = m_meshRenderer.material;
    }

    public void UpdateMaterial(Material material)
    {
        m_meshRenderer.material = material;
    }

    public void SetStatus(TileStatus status)
    {
        m_tileStatus = status;

        if (!string.IsNullOrEmpty(m_tileStatus.PendingOwnerPlayerId))
        {
            Player pendingPlayer = GameClient.Instance.GameWorld.GetPlayerFromId(status.PendingOwnerPlayerId);
            
            if(pendingPlayer != null)
                UpdateMaterial(pendingPlayer.PlayerData.TrailMaterial);
        }
        else if(!string.IsNullOrEmpty(m_tileStatus.OwnerPlayerId))
        {
            Player ownerPlayer = GameClient.Instance.GameWorld.GetPlayerFromId(status.OwnerPlayerId);
            
            if(ownerPlayer != null)
                UpdateMaterial(ownerPlayer.PlayerData.TerritoryMaterial);
        }
        else
        {
            UpdateMaterial(m_defaultMaterial);
        }
    }
}
