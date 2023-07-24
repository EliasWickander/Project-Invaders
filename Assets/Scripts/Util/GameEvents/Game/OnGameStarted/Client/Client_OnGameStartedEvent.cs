using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnGameStartedEvent", menuName = "Custom/Game Events/Client/OnGameStartedEvent")]
public class Client_OnGameStartedEvent : GameEvent<OnGameStartedEventData> { }