using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Data/GameModes/TimedGameMode Data", fileName = "New Timed GameMode Data")]
public class TimedGameModeData : GameModeData
{
    [SerializeField] 
    private float m_roundTimeInSec = 60.0f;
}
