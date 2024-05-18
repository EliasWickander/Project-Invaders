using System;
using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

public class NetworkPlayerTilesMessenger : NetworkBehaviour, INetworkClientMessenger<NetworkPlayerTilesInput, NetworkPlayerTilesState>
{
    public event Action<NetworkPlayerTilesInput> OnInputReceived;

    private NetworkPlayerTilesState m_latestServerState;
    public NetworkPlayerTilesState LatestServerState => m_latestServerState;
	
    public void SendInputToServer(NetworkPlayerTilesInput input)
    {
        CmdSendInputToServer(input);
    }

    public void SendStateToClient(NetworkPlayerTilesState state)
    {
        RpcSendStateToClient(state);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendInputToServer(NetworkPlayerTilesInput input)
    {
        OnInputReceived?.Invoke(input);
    }
	
    [ClientRpc(channel = Channels.Unreliable)]
    private void RpcSendStateToClient(NetworkPlayerTilesState state)
    {
        m_latestServerState = state;
    }
}
