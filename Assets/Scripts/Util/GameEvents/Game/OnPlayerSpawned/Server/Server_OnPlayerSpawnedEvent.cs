using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Server_OnPlayerSpawnedEvent", menuName = "Custom/Game Events/Server/OnPlayerSpawnedEvent")]
public class Server_OnPlayerSpawnedEvent : GameEvent<OnPlayerSpawnedGameEventData> { }