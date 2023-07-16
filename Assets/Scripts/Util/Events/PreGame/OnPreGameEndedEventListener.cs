using CustomToolkit.Events;
using UnityEngine;

public class OnPreGameEndedEventListener : GameEventListener<OnPreGameEndedEventData, OnPreGameEndedEvent>
{
    [SerializeField] 
    private ConnectionType m_listenType;

    [SerializeField] 
    private SubjectType m_subjectType;

    public override void OnEventRaised(OnPreGameEndedEventData value)
    {
        if(m_subjectType == SubjectType.Self && !value.m_isOwned)
            return;
        
        if(m_subjectType == SubjectType.Other && value.m_isOwned)
            return;
        
        if (m_listenType != ConnectionType.Both)
        {
            if (m_listenType != value.m_connectionType)
                return;
        }

        base.OnEventRaised(value);
    }
}
