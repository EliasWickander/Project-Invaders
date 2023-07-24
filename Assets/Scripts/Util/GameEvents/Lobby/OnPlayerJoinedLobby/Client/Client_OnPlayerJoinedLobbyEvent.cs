using CustomToolkit.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "Client_OnPlayerJoinedLobbyEvent", menuName = "Custom/Game Events/Client/OnPlayerJoinedLobbyEvent")]
public class Client_OnPlayerJoinedLobbyEvent : GameEvent<OnPlayerJoinedLobbyEventData> { }