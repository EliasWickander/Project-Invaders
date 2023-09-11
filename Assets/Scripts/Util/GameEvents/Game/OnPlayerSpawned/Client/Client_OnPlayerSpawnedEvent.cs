using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPlayerSpawnedEvent", menuName = "Custom/Game Events/Client/OnPlayerSpawnedEvent")]
public class Client_OnPlayerSpawnedEvent : GameEvent<OnPlayerSpawnedGameEventData> { }