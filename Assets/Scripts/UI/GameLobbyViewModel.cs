using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CustomToolkit.UnityMVVM;
using Mirror;
using TMPro;
using UnityEngine;

[Binding]
public class GameLobbyViewModel : ViewModelMonoBehaviour
{
    public PlayerLobbyEntryViewModel[] m_playerEntries;

    private LobbyRoomPlayer m_localPlayer;

    private PropertyChangedEventArgs m_isLocalPlayerHostProp = new PropertyChangedEventArgs(nameof(IsLocalPlayerHost));
    private bool m_isLocalPlayerHost;

    [Binding]
    public bool IsLocalPlayerHost
    {
        get
        {
            return m_isLocalPlayerHost;
        }
        set
        {
            m_isLocalPlayerHost = value;
            OnPropertyChanged(m_isLocalPlayerHostProp);
        }
    }
    
    private PropertyChangedEventArgs m_isLocalPlayerReadyProp = new PropertyChangedEventArgs(nameof(IsLocalPlayerReady));
    private bool m_isLocalPlayerReady = false;

    [Binding]
    public bool IsLocalPlayerReady
    {
        get
        {
            return m_isLocalPlayerReady;
        }
        set
        {
            m_isLocalPlayerReady = value;
            OnPropertyChanged(m_isLocalPlayerReadyProp);
        }
    }

    private PropertyChangedEventArgs m_canStartProp = new PropertyChangedEventArgs(nameof(CanStart));
    private bool m_canStart = false;

    [Binding]
    public bool CanStart
    {
        get
        {
            return m_canStart;
        }
        set
        {
            m_canStart = value;
            OnPropertyChanged(m_canStartProp);
        }
    }
    public void OnPlayerJoinedLobby(OnPlayerJoinedLobbyEventData data)
    {
        if(data.m_player == null)
            return;

        if (data.m_player.isOwned)
        {
            m_localPlayer = data.m_player;
            IsLocalPlayerHost = m_localPlayer.IsLeader;
        }
        
        Sync();
    }

    public void OnPlayerDisconnect(OnClientDisconnectedEventData data)
    {
        Sync();
    }
    
    public void OnPlayerDisplayNameChanged(OnPlayerDisplayNameChangedEventData data)
    {
        m_playerEntries[data.m_playerIndex].DisplayName = data.m_newDisplayName;
    }

    public void OnPlayerReadyStatusChanged(OnPlayerReadyStatusChangedEventData data)
    {
        m_playerEntries[data.m_playerIndex].IsReady = data.m_newReadyStatus;

        if (m_localPlayer != null)
        {
            if (data.m_playerIndex == m_localPlayer.PlayerIndex)
                IsLocalPlayerReady = data.m_newReadyStatus;   
        }
        
        UpdateCanStart();
    }

    private void Sync()
    {
        UpdateEntries();
        UpdateCanStart();
    }

    private void UpdateCanStart()
    {
        NetworkManagerCustom networkManager = NetworkManagerCustom.Instance;

        //Can't start if too few players in lobby
        if (networkManager.numPlayers < networkManager.MinPlayers)
        {
            CanStart = false;
            return;
        }
        
        //Can't start if at least one player isn't ready
        List<LobbyRoomPlayer> players = NetworkManagerCustom.Instance.LobbyPlayers;
        
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].IsReady == false)
            {
                CanStart = false;
                return;
            }
        }

        CanStart = true;
    }
    private void UpdateEntries()
    {
        //Clear all entries
        foreach (var playerEntry in m_playerEntries)
        {
            playerEntry.Reset();    
        }
        
        List<LobbyRoomPlayer> players = NetworkManagerCustom.Instance.LobbyPlayers;

        for (int i = 0; i < players.Count; i++)
        {
            LobbyRoomPlayer player = players[i];
            PlayerLobbyEntryViewModel entry = m_playerEntries[i];
            
            entry.SetPlayerOwner(player);
        }
    }

    [Binding]
    public void ToggleReady()
    {
        m_localPlayer.ToggleReadyCommand();
    }

    [Binding]
    public void StartGame()
    {
        m_localPlayer.StartGameCommand();
    }
}
