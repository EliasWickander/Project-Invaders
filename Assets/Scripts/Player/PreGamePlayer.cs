using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PreGamePlayer : NetworkBehaviour
{
    [SyncVar]
    private string m_displayName;

    public string DisplayName => m_displayName;
    
    public override void OnStartClient()
    {
        NetworkManagerCustom.Instance.OnPlayerJoinedPreGame(this);
    }

    public override void OnStopClient()
    {
        NetworkManagerCustom.Instance.PreGamePlayers.Remove(this);
    }
    
    [Server]
    public void SetDisplayName(string displayName)
    {
        m_displayName = displayName;
    }
}
