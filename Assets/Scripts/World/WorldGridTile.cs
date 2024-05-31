using System;
using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class TileStatusChangedEventArgs
{
    public TileStatus m_oldStatus;
    public TileStatus m_newStatus;
}

public class WorldGridTile : WorldGridNode
{
    [SerializeField] 
    private MeshRenderer m_meshRenderer;

    private TileStatus m_tileStatus = new TileStatus();
    public TileStatus TileStatus => m_tileStatus;

    private Material m_defaultMaterial;
    
    public event EventHandler<TileStatusChangedEventArgs> OnStatusChanged;
    
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
        TileStatus oldStatus = m_tileStatus;
        m_tileStatus = status;

        if(oldStatus.Equals(m_tileStatus))
            return;
        
        if (!string.IsNullOrEmpty(m_tileStatus.PendingOwnerPlayerId))
        {
            Player pendingPlayer = GameClient.Instance.GameWorld.GetPlayerFromId(status.PendingOwnerPlayerId);

            if (pendingPlayer == null)
            {
                Debug.LogError($"Failed to set status of tile. Attempted to associate node with player that doesn't exist.");
                return;
            }
            
            if(pendingPlayer != null)
                UpdateMaterial(pendingPlayer.PlayerData.TrailMaterial);
        }
        else if(!string.IsNullOrEmpty(m_tileStatus.OwnerPlayerId))
        {
            Player ownerPlayer = GameClient.Instance.GameWorld.GetPlayerFromId(status.OwnerPlayerId);

            if (ownerPlayer == null)
            {
                Debug.LogError($"Failed to set status of tile. Attempted to associate node with player that doesn't exist.");
                return;
            }
            
            if(ownerPlayer != null)
                UpdateMaterial(ownerPlayer.PlayerData.TerritoryMaterial);
        }
        else
        {
            UpdateMaterial(m_defaultMaterial);
        }
        
        OnStatusChanged?.Invoke(this, new TileStatusChangedEventArgs() {m_oldStatus = oldStatus, m_newStatus = m_tileStatus});
    }
}
