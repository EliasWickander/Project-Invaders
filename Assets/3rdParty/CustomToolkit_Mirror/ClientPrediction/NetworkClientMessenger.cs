using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TransmissionData
{
    public int m_currentDataIndex; //Current position in the array of data already received.
    public byte[] m_data;

    public TransmissionData(byte[] data)
    {
        m_currentDataIndex = 0;
        m_data = data;
    }
}

/// <summary>
/// Network Client Messenger. Handles messages sent between server & client. Should be inherited from for specific message types
/// </summary>
public abstract class NetworkClientMessenger : NetworkBehaviour
{
    [Tooltip("The max message size that can be transmitted. If message goes above this, message will be sent in chunks")]
    [SerializeField] 
    private int m_maxMessageSize = 1195;
    
    private List<int> m_serverPendingTransmissionIds = new List<int>();
    private Dictionary<int, TransmissionData> m_pendingReceivedData = new Dictionary<int, TransmissionData>();
    
    protected abstract void OnReceivedMessageOnClient(byte[] messageBuffer);
    protected abstract void OnReceivedMessageOnServer(byte[] messageBuffer);
    
    [Server]
    //TODO: Optimization: Only send a separate prepare message & split up into chunks if message is too big. Otherwise just handle it normally
    protected IEnumerator SendBytesToClient(int transmissionId, byte[] data)
    {
        Debug.Assert(!m_serverPendingTransmissionIds.Contains(transmissionId));

        int fullDataSize = data.Length;
        
        //Prepare client for data
        RpcPrepareToReceiveBytesOnClient(transmissionId, fullDataSize);
        yield return null;
		
        m_serverPendingTransmissionIds.Add(transmissionId);
        TransmissionData dataToTransmit = new TransmissionData(data);
        int bufferSize = m_maxMessageSize;

        //Split up data in chunks if too big
        while (dataToTransmit.m_currentDataIndex < fullDataSize - 1)
        {
            int remainingSize = fullDataSize - dataToTransmit.m_currentDataIndex;
            if (remainingSize < bufferSize)
                bufferSize = remainingSize;

            byte[] buffer = new byte[bufferSize];
            Array.Copy(dataToTransmit.m_data, dataToTransmit.m_currentDataIndex, buffer, 0, bufferSize);
			
            //Send chunk 
            RpcReceiveBytesOnClient(transmissionId, buffer);
            dataToTransmit.m_currentDataIndex += bufferSize;
			
            yield return null;
        }

        //Transmission complete
        m_serverPendingTransmissionIds.Remove(transmissionId);
    }
    
    [ClientRpc(channel = Channels.Reliable)]
    private void RpcPrepareToReceiveBytesOnClient(int transmissionId, int expectedSize)
    {
        if(m_pendingReceivedData.ContainsKey(transmissionId))
            return;

        if(GameDebug.s_debugNetworkMessages)
            Debug.Log($"Preparing to receive bytes associated with transmission id {transmissionId}. Expected size: {expectedSize}");
		
        //Create a temporary container to paste in the byte chunks into until we have the final message
        TransmissionData receivingData = new TransmissionData(new byte[expectedSize]);
        m_pendingReceivedData.Add(transmissionId, receivingData);
    }

    [ClientRpc(channel = Channels.Reliable)]
    private void RpcReceiveBytesOnClient(int transmissionId, byte[] buffer)
    {
        if(!m_pendingReceivedData.ContainsKey(transmissionId))
            return;
        
        if(GameDebug.s_debugNetworkMessages)
            Debug.Log($"Receiving bytes associated with transmission id {transmissionId}. Size: {buffer.Length}");
        
        //Paste message chunk into the prepared transmission data container bit by bit until entire buffer is filled
        TransmissionData dataToReceive = m_pendingReceivedData[transmissionId];
        Array.Copy(buffer, 0, dataToReceive.m_data, dataToReceive.m_currentDataIndex, buffer.Length);
        dataToReceive.m_currentDataIndex += buffer.Length;
		
        //Finished receiving all data associated with transmission id
        if (dataToReceive.m_currentDataIndex >= dataToReceive.m_data.Length)
        {
            OnReceivedMessageOnClient(dataToReceive.m_data);
            m_pendingReceivedData.Remove(transmissionId);
            
            if(GameDebug.s_debugNetworkMessages)
                Debug.Log($"Finished receiving bytes associated with transmission id {transmissionId}");
        }
    }
    
    //TODO: Could also be split up into chunks
    [Client]
    protected void SendBytesToServer(byte[] data)
    {
        CmdSendBytesToServer(data);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendBytesToServer(byte[] buffer)
    {
        OnReceivedMessageOnServer(buffer);
    }
    
    //Serialize message for transmission
    protected byte[] Serialize<T>(T value)
    {
        using (NetworkWriterPooled writer = NetworkWriterPool.Get())
        {
            writer.Write(value);

            return writer.ToArray();
        }
    }
	
    //Deserialize message from a byte array
    protected static T Deserialize<T>(byte[] data)
    {
        using (NetworkReaderPooled reader = NetworkReaderPool.Get(data))
        {
            return reader.Read<T>();
        }
    }
}
