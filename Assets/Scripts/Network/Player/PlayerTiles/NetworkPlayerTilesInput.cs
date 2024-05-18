using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Mirror;
using UnityEngine;

public struct NetworkPlayerTilesInput : INetworkClientInput
{
    public uint m_tick;
    public uint Tick => m_tick;

    public Vector2Int[] m_ownedTiles;
    public Vector2Int[] m_pendingTiles;
    
    public NetworkPlayerTilesInput(uint tick, Vector2Int[] ownedTiles, Vector2Int[] pendingTiles)
    {
        m_tick = tick;
        m_ownedTiles = ownedTiles;
        m_pendingTiles = pendingTiles;
    }
}
