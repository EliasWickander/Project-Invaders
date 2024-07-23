using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameModeType
{
    None,
    Timed,
}

public abstract class GameModeData : ScriptableObject
{
    [SerializeField] 
    private string m_displayName;
    public string DisplayName => m_displayName;
    
    [SerializeField] 
    private GameModeType m_type = GameModeType.None;
    public GameModeType Type => m_type;
    
    [SerializeField] 
    private int m_maxPlayers = 4;
    public int MaxPlayers => m_maxPlayers;
}
