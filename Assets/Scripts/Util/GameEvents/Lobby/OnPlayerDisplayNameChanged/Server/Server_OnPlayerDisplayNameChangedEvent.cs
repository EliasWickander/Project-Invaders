using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerDisplayNameChangedEvent", menuName = "Custom/Game Events/Server/OnPlayerDisplayNameChangedEvent")]
public class Server_OnPlayerDisplayNameChangedEvent : GameEvent<OnPlayerDisplayNameChangedEventData> { }