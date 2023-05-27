using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public PlayerLobbyEntryViewModel[] m_playerEntries;

    private LobbyRoomPlayer m_localPlayer;
    
    public void OnPlayerJoinedLobby(OnPlayerJoinedLobbyEventData data)
    {
        if(data.m_player == null)
            return;

        if (data.m_player.isOwned)
            m_localPlayer = data.m_player;
        
        UpdateEntries();
    }

    public void OnPlayerDisplayNameChanged(OnPlayerDisplayNameChangedEventData data)
    {
        m_playerEntries[data.m_playerIndex].DisplayName = data.m_newDisplayName;
    }

    public void OnPlayerReadyStatusChanged(OnPlayerReadyStatusChangedEventData data)
    {
        m_playerEntries[data.m_playerIndex].IsReady = data.m_newReadyStatus;
    }
    
    private void UpdateEntries()
    {
        //Clear all entries
        foreach (var playerEntry in m_playerEntries)
        {
            playerEntry.Reset();    
        }
        
        List<LobbyRoomPlayer> players = NetworkManagerCustom.Instance.RoomPlayers;

        for (int i = 0; i < players.Count; i++)
        {
            LobbyRoomPlayer player = players[i];
            PlayerLobbyEntryViewModel entry = m_playerEntries[i];
            
            entry.SetPlayerOwner(player);
        }
    }
}
