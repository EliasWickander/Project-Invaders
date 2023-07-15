using System.Collections;
using System.Collections.Generic;
using CustomToolkit.AdvancedTypes;
using UnityEngine;

public class GameClient : Singleton<GameClient>
{
    [SerializeField] 
    private GameData m_gameData;

    public GameData GameData => m_gameData;
}
