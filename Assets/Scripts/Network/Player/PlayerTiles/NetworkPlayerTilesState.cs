using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Mirror;
using UnityEngine;

public struct NetworkPlayerTilesState : INetworkClientState
{
    public Vector2Int[] m_ownedTiles;
    public Vector2Int[] m_pendingTiles;
    
    public uint m_tick;
    public uint Tick => m_tick;

    public bool Equals(NetworkPlayerTilesState other)
    {
        return m_pendingTiles == other.m_pendingTiles && m_ownedTiles == other.m_ownedTiles;
    }
	
    public bool Equals(INetworkClientState other)
    {
        return other is NetworkPlayerTilesState _other && Equals(_other);
    }
}
