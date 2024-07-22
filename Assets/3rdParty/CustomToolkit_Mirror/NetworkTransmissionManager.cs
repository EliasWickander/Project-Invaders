
/// <summary>
/// Keeps track of transmission ids
/// </summary>
public class NetworkTransmissionManager
{
    private int m_lastTransmissionId = -1;

    public void Clear()
    {
        m_lastTransmissionId = -1;
    }
    
    public int GetNextTransmissionId()
    {
        m_lastTransmissionId++;
        return m_lastTransmissionId;
    }
}
