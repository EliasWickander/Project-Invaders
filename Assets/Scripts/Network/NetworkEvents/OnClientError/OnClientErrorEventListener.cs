using Mirror;
using CustomToolkit.Events;
public class OnClientErrorEventListener : GameEventListener<TransportError, OnClientErrorEvent>
{
    public bool m_singleErrorListener = false;
    public TransportError m_errorTypeToListenFor;

    public override void OnEventRaised(TransportError value)
    {
        if(m_singleErrorListener && value != m_errorTypeToListenFor)
            return;
        
        base.OnEventRaised(value);
    }
}
