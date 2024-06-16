using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using CustomToolkit.Mirror;
using Mirror;
using UnityEngine;

public class TransmissionData
{
	public int curDataIndex; //current position in the array of data already received.
	public byte[] data;

	public TransmissionData(byte[] _data){
		curDataIndex = 0;
		data = _data;
	}
}

public class NetworkPlayerMessenger : NetworkBehaviour, INetworkClientMessenger<NetworkPlayerInput, NetworkPlayerState>
{
	public event Action<NetworkPlayerInput> OnInputReceived;

	public NetworkPlayerState LatestServerState { get; set; }

	public void SendInputToServer(NetworkPlayerInput input)
	{
		if (ServerDebug.s_debugPackages)
			Debug.Log($"Sending input package to client: {input.Log()}");
		
		byte[] inputBuffer = Serialize<NetworkPlayerInput>(input);
		
		CmdSendInputToServer(inputBuffer);
	}

	public void SendStateToClient(NetworkPlayerState state)
	{
		if (ServerDebug.s_debugPackages)
			Debug.Log($"Sending state package to client: {state.Log()}");

		byte[] stateBuffer = Serialize<NetworkPlayerState>(state);
		
		RpcSendStateToClient(stateBuffer);
	}

	[Command(channel = Channels.Unreliable)]
	private void CmdSendInputToServer(byte[] inputBuffer)
	{
		NetworkPlayerInput receivedInput = Deserialize<NetworkPlayerInput>(inputBuffer);
		
		OnInputReceived?.Invoke(receivedInput);
	}
	
	[ClientRpc(channel = Channels.Unreliable)]
	private void RpcSendStateToClient(byte[] stateBuffer)
	{
		NetworkPlayerState receivedState = Deserialize<NetworkPlayerState>(stateBuffer);
		
		if (ServerDebug.s_debugPackages)
			Debug.Log($"Received state package on client: {receivedState.Log()}");	
		
		//Ignore outdated server states
		if (receivedState.Tick <= LatestServerState.Tick)
		{
			if (ServerDebug.s_debugPackages)
				Debug.Log($"Ignored package due to being out of date. Tick: {receivedState.Tick}. Latest server tick: {LatestServerState.Tick}");	
			
			return;	
		}

		LatestServerState = receivedState;
	}

	// Serialize message for transmission
	public byte[] Serialize<T>(T value)
	{
		using (NetworkWriterPooled writer = NetworkWriterPool.Get())
		{
			writer.Write(value);

			return writer.ToArray();
		}
	}
	
	// Deserialize message from a byte array
	public static T Deserialize<T>(byte[] data)
	{
		using (NetworkReaderPooled reader = NetworkReaderPool.Get(data))
		{
			return reader.Read<T>();
		}
	}
}