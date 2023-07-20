using System.Collections;
using System.Collections.Generic;
using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "OnPlayerJoinedLobby", menuName = "Custom/NetworkEvents/OnPlayerJoinedLobbyEvent")]
public class OnPlayerJoinedLobbyEvent : NetworkGameEvent<OnPlayerJoinedLobbyEventData> { }
