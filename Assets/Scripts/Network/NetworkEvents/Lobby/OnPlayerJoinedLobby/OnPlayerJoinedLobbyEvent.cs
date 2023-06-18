using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnPlayerJoinedLobby", menuName = "Custom/Events/Network/OnPlayerJoinedLobbyEvent")]
public class OnPlayerJoinedLobbyEvent : GameEvent<OnPlayerJoinedLobbyEventData> { }
