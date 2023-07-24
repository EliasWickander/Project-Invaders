using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnGameStartedEvent", menuName = "Custom/Game Events/Server/OnGameStartedEvent")]
public class Server_OnGameStartedEvent : GameEvent<OnGameStartedEventData> { }