using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerJoinedGameEvent", menuName = "Custom/Game Events/Server/OnPlayerJoinedGameEvent")]
public class Server_OnPlayerJoinedGameEvent : GameEvent<OnPlayerJoinedGameEventData> { }