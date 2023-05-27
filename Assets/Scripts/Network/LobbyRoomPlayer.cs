using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomPlayer : NetworkBehaviour
{
    [SerializeField] 
    private OnPlayerOrderChangedEvent m_onPlayerOrderChangedEvent;
    
    [SerializeField] 
    private OnPlayerDisplayNameChangedEvent m_onDisplayNameChangedEvent;

    [SerializeField] 
    private OnPlayerReadyStatusChangedEvent m_onReadyStatusChangedEvent;

    [SyncVar(hook = nameof(OnPlayerOrderChanged))]
    public int Order = 0;
    
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
            SetOrder(Lobby.RoomPlayers.Count - 1);

            SetDisplayNameCommand(PlayerPrefs.GetString(PlayerNameInput.c_playerPrefsDisplayNameKey));   
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
            m_onReadyStatusChangedEvent.Raise(new OnPlayerReadyStatusChangedEventData() {m_playerIndex = Order, m_oldReadyStatus = oldValue, m_newReadyStatus = newValue});
    }

    public void OnDisplayNameChanged(string oldValue, string newValue)
    {
        if(m_onDisplayNameChangedEvent != null)
            m_onDisplayNameChangedEvent.Raise(new OnPlayerDisplayNameChangedEventData() {m_playerIndex = Order, m_oldDisplayName = oldValue, m_newDisplayName = newValue});
    }

    [Command]
    public void SetOrder(int order)
    {
        Order = order;
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
        
        Lobby.NotifyPlayersReadyState();
    }

    [Command]
    public void StartGameCommand()
    {
        if(Lobby.RoomPlayers[0].connectionToClient != connectionToClient)
            return;
        
        //start game
    }
    public void HandleReadyToStart(bool readyToStart)
    {
        if(!IsLeader)
            return;

    }
}