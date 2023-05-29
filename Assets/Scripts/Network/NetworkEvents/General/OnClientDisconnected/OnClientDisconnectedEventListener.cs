using CustomToolkit.Events;
public class OnClientDisconnectedEventListener : GameEventListener<OnClientDisconnectedEventData, OnClientDisconnectedEvent>
{
    public enum SubjectType
    {
        Self,
        Other,
    }

    public enum Scope
    {
        Local,
        Global
    }

    public SubjectType m_listenType;
    public Scope m_scope;
    
    public override void OnEventRaised(OnClientDisconnectedEventData value)
    {
        if(m_scope == Scope.Local && !value.m_isLocalScope)
            return;
        
        if(m_scope == Scope.Global && value.m_isLocalScope)
            return;
        
        if (m_listenType == SubjectType.Self)
        {
            if(!value.m_wasSelf)
                return;
        }
        else if (m_listenType == SubjectType.Other)
        {
            if(value.m_wasSelf)
                return;
        }
        
        base.OnEventRaised(value);
    }
}
