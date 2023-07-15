using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnPlayerJoinedPreGame", menuName = "Custom/Events/Network/OnPlayerJoinedPreGameEvent")]
public class OnPlayerJoinedPreGameEvent : GameEvent<OnPlayerJoinedPreGameEventData> { }