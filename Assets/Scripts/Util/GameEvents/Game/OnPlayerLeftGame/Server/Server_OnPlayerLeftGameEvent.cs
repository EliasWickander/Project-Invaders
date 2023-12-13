using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerLeftGameEvent", menuName = "Custom/Game Events/Server/OnPlayerLeftGameEvent")]
public class Server_OnPlayerLeftGameEvent : GameEvent<OnPlayerLeftGameEventData> { }