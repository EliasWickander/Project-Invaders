using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerJoinedLobbyEvent", menuName = "Custom/Game Events/Server/OnPlayerJoinedLobbyEvent")]
public class Server_OnPlayerJoinedLobbyEvent : GameEvent<OnPlayerJoinedLobbyEventData> { }