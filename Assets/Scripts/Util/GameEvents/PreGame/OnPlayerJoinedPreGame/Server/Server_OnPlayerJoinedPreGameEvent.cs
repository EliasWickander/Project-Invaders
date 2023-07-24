using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerJoinedPreGameEvent", menuName = "Custom/Game Events/Server/OnPlayerJoinedPreGameEvent")]
public class Server_OnPlayerJoinedPreGameEvent : GameEvent<OnPlayerJoinedPreGameEventData> { }