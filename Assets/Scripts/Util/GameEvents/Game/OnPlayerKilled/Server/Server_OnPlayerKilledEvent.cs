using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerKilledEvent", menuName = "Custom/Game Events/Server/OnPlayerKilledEvent")]
public class Server_OnPlayerKilledEvent : GameEvent<OnPlayerKilledGameEventData> { }