using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyRoomPlayer : NetworkBehaviour
{
    [SerializeField] 
    private OnPlayerOrderChangedEvent m_onPlayerOrderChangedEvent;

    [SerializeField] 
    private OnClientDisconnectedEvent m_onClientDisconnectedEvent;

    [SerializeField] 
    private OnPlayerDisplayNameChangedEvent m_onDisplayNameChangedEvent;

    [SerializeField] 
    private OnPlayerReadyStatusChangedEvent m_onReadyStatusChangedEvent;

    [SyncVar(hook = nameof(OnPlayerOrderChanged))]
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
    
    private NetworkManagerCustom m_lobby;

    private NetworkManagerCustom Lobby
    {
        get
        {
            if (m_lobby != null)
                return m_lobby;
            
            m_lobby = NetworkManager.singleton as NetworkManagerCustom;
            return m_lobby;
        }
    }

    public override void OnStartClient()
    {
        Lobby.OnPlayerJoinedLobby(this);

        if (isOwned)
        {
            SetPlayerIndex(Lobby.RoomPlayers.Count - 1);

            SetDisplayNameCommand(PlayerPrefs.GetString(PlayerNameInput.c_playerPrefsDisplayNameKey));   
            
            if(IsLeader)
                ToggleReadyCommand();
        }
    }

    public override void OnStopClient()
    {
        Lobby.RoomPlayers.Remove(this);
    }

    public void OnPlayerOrderChanged(int oldValue, int newValue)
    {
        if(m_onPlayerOrderChangedEvent != null)
            m_onPlayerOrderChangedEvent.Raise(new OnPlayerOrderChangedEventData() {m_playerIndex = oldValue, m_oldOrder = oldValue, m_newOrder = newValue});
    }
    
    public void OnReadyStatusChanged(bool oldValue, bool newValue)
    {
        if(m_onReadyStatusChangedEvent != null)
            m_onReadyStatusChangedEvent.Raise(new OnPlayerReadyStatusChangedEventData() {m_playerIndex = PlayerIndex, m_oldReadyStatus = oldValue, m_newReadyStatus = newValue});
    }

    public void OnDisplayNameChanged(string oldValue, string newValue)
    {
        if(m_onDisplayNameChangedEvent != null)
            m_onDisplayNameChangedEvent.Raise(new OnPlayerDisplayNameChangedEventData() {m_playerIndex = PlayerIndex, m_oldDisplayName = oldValue, m_newDisplayName = newValue});
    }

    [Command]
    public void SetPlayerIndex(int order)
    {
        PlayerIndex = order;
    }
    [Command]
    public void SetDisplayNameCommand(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void ToggleReadyCommand()
    {
        IsReady = !IsReady;
    }

    [Command]
    public void StartGameCommand()
    {
        if(!IsLeader)
            return;

        NetworkManagerCustom.Instance.StartGame();
    }

    [ClientRpc]
    public void HandleClientDisconnected(int clientIndex)
    {
        if(m_onClientDisconnectedEvent != null)
            m_onClientDisconnectedEvent.Raise(new OnClientDisconnectedEventData() {m_wasSelf = false, m_isLocalScope = isOwned});

        if (PlayerIndex > clientIndex)
            PlayerIndex--;
    }
}