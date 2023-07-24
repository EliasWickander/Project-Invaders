using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerReadyStatusChangedEvent", menuName = "Custom/Game Events/Server/OnPlayerReadyStatusChangedEvent")]
public class Server_OnPlayerReadyStatusChangedEvent : GameEvent<OnPlayerReadyStatusChangedEventData> { }