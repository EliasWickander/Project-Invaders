using System;

[Serializable]
public class OnPlayerReadyStatusChangedEventData
{
    public int m_playerIndex;
    public bool m_newReadyStatus;
}
