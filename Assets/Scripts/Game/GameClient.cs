using System.Collections;
using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using Mirror;
using UnityEngine;

public class GameClient : Singleton<GameClient>
{
    [SerializeField] 
    private GameData m_gameData;

    public GameData GameData => m_gameData;

    public GameWorld GameWorld { get; private set; }
    
    public NetworkTransmissionManager NetworkTransmissionManager { get; private set; }

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();

        GameWorld = new GameWorld();
        NetworkTransmissionManager = new NetworkTransmissionManager();
    }
}
