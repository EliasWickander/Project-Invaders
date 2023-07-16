using System;
using Mirror;

[Serializable]
public class OnGameStartedEventData
{
    public bool m_isOwned;
    public ConnectionType m_connectionType;
}
