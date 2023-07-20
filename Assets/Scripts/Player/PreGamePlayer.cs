using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class PreGamePlayer : NetworkBehaviour
{
    [SyncVar]
    public string DisplayName = "";

    [SyncVar] 
    public bool HasSelectedElement = false;

    [SerializeField] 
    private OnPreGameEndedEvent m_onPreGameEndedEvent;
    
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

        NetworkManagerCustom networkManager = NetworkManagerCustom.Instance;
        
        if(networkManager.CanStartGame())
            networkManager.StartGame();
    }

    [ClientRpc]
    public void OnGameStarted()
    {
        if(m_onPreGameEndedEvent != null)
            m_onPreGameEndedEvent.Raise(new OnPreGameEndedEventData() {});
    }
}
