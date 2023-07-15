using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

public class OnPlayerJoinedLobbyEventListener : GameEventListener<OnPlayerJoinedLobbyEventData, OnPlayerJoinedLobbyEvent>
{
    public enum SubjectType
    {
        Both,
        Self,
        Other,
    }

    public SubjectType m_subject;

    public override void OnEventRaised(OnPlayerJoinedLobbyEventData value)
    {
        if(m_subject == SubjectType.Self && !value.m_player.isOwned)
            return;
        
        if(m_subject == SubjectType.Other && value.m_player.isOwned)
            return;

        base.OnEventRaised(value);
    }
}