using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyRoomPlayer : NetworkBehaviour
{
    [SerializeField] 
    private Client_OnClientDisconnectedEvent m_onClientDisconnectedClientEvent;

    [SerializeField] 
    private Client_OnPlayerDisplayNameChangedEvent m_onDisplayNameChangedClientEvent;
    
    [SerializeField] 
    private Server_OnPlayerDisplayNameChangedEvent m_onDisplayNameChangedServerEvent;

    [SerializeField] 
    private Client_OnPlayerReadyStatusChangedEvent m_onPlayerReadyStatusChangedClientEvent;
    
    [SerializeField] 
    private Server_OnPlayerReadyStatusChangedEvent m_onPlayerReadyStatusChangedServerEvent;

    //
    [SyncVar]
    public int PlayerIndex = -1;
    
    [SyncVar(hook = nameof(OnDisplayNameChanged))]
    public string DisplayName = "Loading...";
    
    [SyncVar(hook = nameof(OnReadyStatusChanged))]
    public bool IsReady = false;

    private bool m_isLeader = false;
    
    public bool IsLeader
    {
        get { return m_isLeader; }
        set
        {
            m_isLeader = value;
        }
    }

    public override void OnStartClient()
    {
        NetworkManagerCustom networkManager = NetworkManagerCustom.Instance;
        
        networkManager.OnPlayerJoinedLobby(this);

        if (isOwned)
        {
            SetPlayerIndex(networkManager.LobbyPlayers.Count - 1);

            SetDisplayNameCommand(PlayerPrefs.GetString(PlayerNameInput.c_playerPrefsDisplayNameKey));   
            
            if(IsLeader)
                ToggleReadyCommand();
        }
    }

    public override void OnStopClient()
    {
        NetworkManagerCustom networkManager = NetworkManagerCustom.Instance;
        
        networkManager.LobbyPlayers.Remove(this);
    }

    public void OnReadyStatusChanged(bool oldValue, bool newValue)
    {
        if(m_onPlayerReadyStatusChangedClientEvent != null)
            m_onPlayerReadyStatusChangedClientEvent.Raise(new OnPlayerReadyStatusChangedEventData() {m_playerIndex = PlayerIndex, m_newReadyStatus = newValue});
    }

    public void OnDisplayNameChanged(string oldValue, string newValue)
    {
        if(m_onDisplayNameChangedClientEvent != null)
            m_onDisplayNameChangedClientEvent.Raise(new OnPlayerDisplayNameChangedEventData() {m_playerIndex = PlayerIndex, m_oldDisplayName = oldValue, m_newDisplayName = newValue});
    }

    [Command]
    public void SetPlayerIndex(int order)
    {
        PlayerIndex = order;
    }
    
    [Command]
    public void SetDisplayNameCommand(string displayName)
    {
        string oldDisplayName = displayName;
        
        DisplayName = displayName;
        
        NetworkManagerCustom networkManager = NetworkManagerCustom.Instance;

        //Update player profile to match
        PlayerProfile playerProfile = networkManager.GetPlayerProfileFromConnection(connectionToClient);
        playerProfile.m_displayName = displayName;
        
        if(m_onDisplayNameChangedServerEvent != null)
            m_onDisplayNameChangedServerEvent.Raise(new OnPlayerDisplayNameChangedEventData() {m_playerIndex = PlayerIndex, m_oldDisplayName = oldDisplayName, m_newDisplayName = displayName});
    }

    [Command]
    public void ToggleReadyCommand()
    {
        IsReady = !IsReady;
        
        if(m_onPlayerReadyStatusChangedServerEvent != null)
            m_onPlayerReadyStatusChangedServerEvent.Raise(new OnPlayerReadyStatusChangedEventData() {m_playerIndex = PlayerIndex, m_newReadyStatus = IsReady});
    }

    [Command]
    public void StartGameCommand()
    {
        if(!IsLeader)
            return;

        NetworkManagerCustom.Instance.StartPreGame();
    }

    [ClientRpc]
    public void HandleClientDisconnected(int clientIndex)
    {
        if(m_onClientDisconnectedClientEvent != null)
            m_onClientDisconnectedClientEvent.Raise(new OnClientDisconnectedEventData());

        if (PlayerIndex > clientIndex)
            PlayerIndex--;
    }
}