using System.Collections;
using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class GameClient : Singleton<GameClient>
{
    [SerializeField] 
    private GameData m_gameData;

    public GameData GameData => m_gameData;
    
    public GameWorld GameWorld { get; private set; }

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();

        GameWorld = new GameWorld();
    }
}
