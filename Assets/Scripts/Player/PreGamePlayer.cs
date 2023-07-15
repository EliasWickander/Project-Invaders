using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PreGamePlayer : NetworkBehaviour
{
    [SyncVar]
    public string DisplayName = "";

    [SyncVar] 
    public bool HasSelectedElement = false;
    
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
        DisplayName = displayName;
    }

    [Command]
    public void SelectElement()
    {
        HasSelectedElement = true;
    }
}
