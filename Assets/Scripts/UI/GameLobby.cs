using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public PlayerLobbyEntryViewModel[] m_playerEntries;

    private void Start()
    {
        UpdateEntries();
    }

    public void OnServerConnected(OnServerConnectedEventData data)
    {
        Debug.Log(data.m_connection.identity);
        if(data.m_connection.identity == null)
            return;
        
        UpdateEntries();
    }

    private void UpdateEntries()
    {
        //Clear all entries
        foreach (var playerEntry in m_playerEntries)
        {
            playerEntry.Reset();    
        }
        
        List<LobbyRoomPlayer> players = NetworkManagerCustom.Instance.RoomPlayers;

        Debug.Log(players.Count);
        for (int i = 0; i < players.Count; i++)
        {
            LobbyRoomPlayer player = players[i];
            PlayerLobbyEntryViewModel entry = m_playerEntries[i];
            
            entry.SetPlayerOwner(player);
        }
    }
}
