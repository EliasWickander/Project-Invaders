using System.Collections;
using System.Collections.Generic;
using CustomToolkit.UnityMVVM;
using UnityEngine;

public class PreGameScreenViewModel : ViewModelMonoBehaviour
{
    private PreGamePlayer m_localPlayer;
    
    public void OnPlayerJoinedPreGame(OnPlayerJoinedPreGameEventData data)
    {
        if(data.m_player == null)
            return;

        if (data.m_player.isOwned)
        {
            m_localPlayer = data.m_player;
        }

        Debug.LogError($"Player joined\n" +
                       $"Display Name: {data.m_player.DisplayName}\n" +
                       $"Connection: {data.m_player.connectionToClient}");
    }
}
