using System;

[Serializable]
public class OnPlayerDisplayNameChangedEventData
{
    public int m_playerIndex;
    public string m_oldDisplayName;
    public string m_newDisplayName;
}
