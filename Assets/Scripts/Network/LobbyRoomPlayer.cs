using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomPlayer : NetworkBehaviour
{
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

    public override void OnStartAuthority()
    {
        SetDisplayNameCommand(PlayerPrefs.GetString(PlayerNameInput.c_playerPrefsDisplayNameKey));
    }

    public override void OnStartClient()
    {
        Lobby.RoomPlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Lobby.RoomPlayers.Remove(this);
    }

    public void OnReadyStatusChanged(bool oldValue, bool newValue)
    {
        
    }

    public void OnDisplayNameChanged(string oldValue, string newValue)
    {
        
    }

    [Command]
    public void SetDisplayNameCommand(string displayName)
    {
        DisplayName = displayName;
        
        Debug.Log("set name " + displayName);
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