using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerCreatedEvent", menuName = "Custom/Game Events/Server/OnPlayerCreatedEvent")]
public class Server_OnPlayerCreatedEvent : GameEvent<OnPlayerCreatedEventData> { }