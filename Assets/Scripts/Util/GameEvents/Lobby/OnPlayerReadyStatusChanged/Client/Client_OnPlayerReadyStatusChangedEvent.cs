using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPlayerReadyStatusChangedEvent", menuName = "Custom/Game Events/Client/OnPlayerReadyStatusChangedEvent")]
public class Client_OnPlayerReadyStatusChangedEvent : GameEvent<OnPlayerReadyStatusChangedEventData> { }