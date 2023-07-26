using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPlayerJoinedGameEvent", menuName = "Custom/Game Events/Client/OnPlayerJoinedGameEvent")]
public class Client_OnPlayerJoinedGameEvent : GameEvent<OnPlayerJoinedGameEventData> { }