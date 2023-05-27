using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPlayerReadyStatusChangedEventData
{
    public LobbyRoomPlayer m_player;
    public bool m_oldReadyStatus;
    public bool m_newReadyStatus;
}
